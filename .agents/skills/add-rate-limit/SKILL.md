---
name: add-rate-limit
description: Adds a new rate limit policy or account key rate limit to the Smart Court project.
---

# Add Rate Limit Skill

When tasked with adding or modifying rate limits in the SmartCourt project, you must first determine which layer of the Defense-in-Depth architecture is required: an **Endpoint Rate Limit** (Layer 1) or an **Account Key Rate Limit** (Layer 2).

## Layer 1: Endpoint Rate Limit (Global Middleware)
Use this layer to protect endpoints from high-volume generic traffic (based on IP Address or User ID).

**Steps:**
1. **Open** `SmartCourt/Common/RateLimiting/SecurityRateLimitPolicy.cs`.
2. **Add a constant** to the `RateLimitPolicyNames` class:
   ```csharp
   public const string YourNewPolicyName = "YourNewPolicyName";
   ```
3. **Add the bucket definition** to the `SecurityRateLimitPolicies.Policies` dictionary:
   ```csharp
   [RateLimitPolicyNames.YourNewPolicyName] = new(
       Ip: new RateLimitBucket(PermitLimit: 100, Window: TimeSpan.FromMinutes(1)),
       User: new RateLimitBucket(PermitLimit: 30, Window: TimeSpan.FromMinutes(1)) // User bucket is optional
   )
   ```
4. **Apply the attribute** to the target Controller endpoint:
   ```csharp
   [SecurityRateLimit(RateLimitPolicyNames.YourNewPolicyName)]
   [HttpGet("your-endpoint")]
   public async Task<IActionResult> HandleAsync() { ... }
   ```

## Layer 2: Account Key Rate Limit (Internal Service Logic)
Use this layer to protect specific user accounts from targeted abuse (e.g., brute-forcing a specific email address across multiple IPs).

**Steps:**
1. **Open** `SmartCourt/Common/RateLimiting/IAccountKeyRateLimiter.cs`.
2. **Add a method signature** for the new action:
   ```csharp
   void CheckYourNewAction(string identifier);
   ```
3. **Open** `SmartCourt/Common/RateLimiting/AccountKeyRateLimiter.cs`.
4. **Define a new partitioned limiter** field at the top of the class:
   ```csharp
   private readonly PartitionedRateLimiter<string> _yourNewActionLimiter =
       CreateLimiter(permitLimit: 5, window: TimeSpan.FromHours(1));
   ```
5. **Implement the check method:**
   ```csharp
   public void CheckYourNewAction(string identifier)
   {
       // IMPORTANT: Use NormalizeEmail(identifier) if the identifier is an email address!
       EnsureAllowed(_yourNewActionLimiter, identifier);
   }
   ```
6. **Dispose the limiter:**
   Ensure you add `_yourNewActionLimiter.Dispose();` inside the `Dispose()` method of the class.
7. **Use it in the Application:**
   Inject `IAccountKeyRateLimiter` into the target Controller or Service and call the check method *before* executing the sensitive business logic.
