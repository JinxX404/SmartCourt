# SmartCourt Rate Limiting: Reference & Implementation Guide

This document serves as a comprehensive reference for the Rate Limiting architecture implemented in the SmartCourt project. It explains the "Defense in Depth" approach taken, how the code works under the hood, and provides step-by-step instructions for developers on how to apply existing limits or expand the system with new ones.

---

## 1. Architectural Overview: Defense in Depth

The system uses a two-layered approach to rate limiting. These layers operate entirely independently and protect against different types of attacks.

1.  **Layer 1: Endpoint Rate Limiting (The Middleware)**
    *   **Scope:** Global. Runs before the controller executes.
    *   **Purpose:** Protects server infrastructure from being overwhelmed by generic high-volume traffic (e.g., noisy neighbors, simple DDoS).
    *   **Identifiers:** Tracks requests by **IP Address** and **User ID** (if authenticated).
2.  **Layer 2: Account Key Rate Limiting (The Service Layer)**
    *   **Scope:** Specific Business Logic. Runs inside controllers or services.
    *   **Purpose:** Protects specific user accounts from targeted attacks (e.g., distributed brute-forcing of a password reset token across 1,000 different IPs).
    *   **Identifiers:** Tracks requests by sensitive keys like **Email Address** or **Reset Tokens**.

> [!TIP]
> **Why both?** If an attacker uses a botnet with thousands of different IP addresses to attack a single user's account, the IP-based Endpoint Limiter (Layer 1) will fail to block them. The Account Key Limiter (Layer 2) catches this by strictly limiting actions against that specific user account, regardless of the IP address.

---

## 2. Layer 1: Endpoint Rate Limiting

This layer is built on ASP.NET Core's native `System.Threading.RateLimiting` middleware. It is configured in `DependencyInjection.cs`.

### How it Works
The middleware uses a "Chained Limiter". When a request hits an endpoint decorated with a rate limit attribute:
1.  It checks the **IP Address** limit. If exceeded, it returns `429 Too Many Requests`.
2.  If the IP check passes, and the user is authenticated, it checks the **User ID** limit. If exceeded, it returns `429 Too Many Requests`.

### How to USE an Existing Policy
To protect an endpoint, simply decorate your Controller method with the `[SecurityRateLimit]` attribute, passing in the policy name.

```csharp
[HttpGet("public-profile")]
[SecurityRateLimit(RateLimitPolicyNames.PublicLawyerGet)] // Applies the limit
public async Task<IActionResult> GetLawyerProfile()
{
    // ... logic ...
}
```

### How to ADD a New Policy
If you are creating a new feature and need a new rate limit policy, follow these steps:

1.  **Open `Common/RateLimiting/SecurityRateLimitPolicy.cs`**
2.  **Add a new constant name:**
    ```csharp
    public static class RateLimitPolicyNames
    {
        // ... existing names ...
        public const string MyNewFeature = "MyNewFeature";
    }
    ```
3.  **Define the bucket limits in the dictionary:**
    You must define an IP limit, and optionally a User limit (which should usually be stricter).
    ```csharp
    public static class SecurityRateLimitPolicies
    {
        private static readonly IReadOnlyDictionary<string, SecurityRateLimitPolicy> Policies =
            new Dictionary<string, SecurityRateLimitPolicy>
            {
                // ... existing policies ...
                [RateLimitPolicyNames.MyNewFeature] = new(
                    Ip: new RateLimitBucket(PermitLimit: 100, Window: TimeSpan.FromMinutes(1)),
                    User: new RateLimitBucket(PermitLimit: 30, Window: TimeSpan.FromMinutes(1))
                )
            };
    }
    ```

---

## 3. Layer 2: Account Key Rate Limiting

This layer uses a custom Singleton service (`AccountKeyRateLimiter`) that maintains highly-optimized, thread-safe memory dictionaries ("Partitions") to track specific values like emails or tokens.

> [!IMPORTANT]
> **Security Note:** This service automatically normalizes (e.g., standardizes casing) and cryptographically hashes (SHA256) the values before storing them. It never stores plain-text emails in the server's RAM rate-limit buckets.

### How to USE an Existing Account Limit
To use this inside a Controller or Service, inject `IAccountKeyRateLimiter` and call the relevant check method *before* performing any heavy business logic.

```csharp
public class ForgotPasswordController(
    IForgotPasswordService service,
    IAccountKeyRateLimiter accountKeyRateLimiter) // 1. Inject
{
    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] Request request)
    {
        // 2. Call the check. If the limit is reached, this throws a TooManyRequestsException
        // which halts execution and returns a 429 response.
        accountKeyRateLimiter.CheckForgotPassword(request.Email); 
        
        // 3. Safe to proceed
        await service.ProcessAsync(request.Email);
        return Ok();
    }
}
```

### How to ADD a New Account Key Limit
If you are building a new sensitive flow (e.g., "Change Phone Number") and need to limit it per-account:

1.  **Update the Interface (`IAccountKeyRateLimiter.cs`):**
    ```csharp
    public interface IAccountKeyRateLimiter
    {
        // ... existing ...
        void CheckChangePhoneNumber(string userId);
    }
    ```

2.  **Add the Limiter Bucket (`AccountKeyRateLimiter.cs`):**
    Create a new permanent "filing cabinet" for this specific action at the top of the class.
    ```csharp
    private readonly PartitionedRateLimiter<string> _changePhoneUserIdLimiter =
        CreateLimiter(permitLimit: 2, window: TimeSpan.FromHours(24));
    ```

3.  **Implement the Check Method:**
    Use `EnsureAllowed` to enforce the limit.
    ```csharp
    public void CheckChangePhoneNumber(string userId)
    {
        EnsureAllowed(_changePhoneUserIdLimiter, userId);
    }
    ```

4.  **Dispose the Limiter:**
    Ensure you clean up memory when the application shuts down.
    ```csharp
    public void Dispose()
    {
        // ... existing ...
        _changePhoneUserIdLimiter.Dispose();
    }
    ```

---

## 4. Exception Handling & Responses

Both layers seamlessly integrate with the SmartCourt `ApiResponse<T>` format.

*   **Layer 1 (Middleware):** If an IP or User hits the limit, the `OnRejected` callback in `DependencyInjection.cs` intercepts the request and instantly writes a `429 Too Many Requests` JSON response.
*   **Layer 2 (Account Key):** If an account hits the limit, `EnsureAllowed` throws a `TooManyRequestsException`. The global exception handling middleware catches this specific exception type and formats it into the standard `429 Too Many Requests` JSON response.
