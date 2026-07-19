namespace SmartCourt.Common.RateLimiting;

[AttributeUsage(AttributeTargets.Method)]
public sealed class SecurityRateLimitAttribute(string policyName) : Attribute
{
    public string PolicyName { get; } = policyName;
}

public static class RateLimitPolicyNames
{
    public const string PrivateProfileGet = "PrivateProfileGet";
    public const string PrivateProfileUpdate = "PrivateProfileUpdate";
    public const string PrivateProfileDelete = "PrivateProfileDelete";
    public const string PublicLawyerGet = "PublicLawyerGet";
    public const string ChangePassword = "ChangePassword";
    public const string ForgotPassword = "ForgotPassword";
    public const string ResendVerification = "ResendVerification";
    public const string ResetPassword = "ResetPassword";
    public const string ConfirmEmail = "ConfirmEmail";
}

public sealed record RateLimitBucket(int PermitLimit, TimeSpan Window);

public sealed record SecurityRateLimitPolicy(RateLimitBucket Ip, RateLimitBucket? User = null);

public static class SecurityRateLimitPolicies
{
    private static readonly IReadOnlyDictionary<string, SecurityRateLimitPolicy> Policies =
        new Dictionary<string, SecurityRateLimitPolicy>
        {
            [RateLimitPolicyNames.PrivateProfileGet] = new(
                new RateLimitBucket(300, TimeSpan.FromMinutes(1)),
                new RateLimitBucket(120, TimeSpan.FromMinutes(1))),
            [RateLimitPolicyNames.PrivateProfileUpdate] = new(
                new RateLimitBucket(60, TimeSpan.FromMinutes(15)),
                new RateLimitBucket(20, TimeSpan.FromMinutes(15))),
            [RateLimitPolicyNames.PrivateProfileDelete] = new(
                new RateLimitBucket(10, TimeSpan.FromDays(1)),
                new RateLimitBucket(3, TimeSpan.FromDays(1))),
            [RateLimitPolicyNames.PublicLawyerGet] = new(
                new RateLimitBucket(120, TimeSpan.FromMinutes(1))),
            [RateLimitPolicyNames.ChangePassword] = new(
                new RateLimitBucket(20, TimeSpan.FromMinutes(15)),
                new RateLimitBucket(5, TimeSpan.FromMinutes(15))),
            [RateLimitPolicyNames.ForgotPassword] = new(
                new RateLimitBucket(5, TimeSpan.FromMinutes(15))),
            [RateLimitPolicyNames.ResendVerification] = new(
                new RateLimitBucket(5, TimeSpan.FromMinutes(15))),
            [RateLimitPolicyNames.ResetPassword] = new(
                new RateLimitBucket(10, TimeSpan.FromMinutes(15))),
            [RateLimitPolicyNames.ConfirmEmail] = new(
                new RateLimitBucket(20, TimeSpan.FromMinutes(15)))
        };

    public static bool TryGet(string policyName, out SecurityRateLimitPolicy policy)
    {
        return Policies.TryGetValue(policyName, out policy!);
    }
}

public static class RateLimitResponse
{
    public const string Message = "Too many requests. Please try again later.";
}
