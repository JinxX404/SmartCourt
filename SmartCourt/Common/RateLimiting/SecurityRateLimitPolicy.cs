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
    public const string AuthenticatedQuery = "AuthenticatedQuery";
    public const string FinancialQuery = "FinancialQuery";
    public const string StandardMutation = "StandardMutation";
    public const string SensitiveMutation = "SensitiveMutation";
    public const string FinancialMutation = "FinancialMutation";
    public const string AdminFinancialMutation = "AdminFinancialMutation";
    public const string PaymentWebhook = "PaymentWebhook";
}

public sealed record RateLimitBucket(int PermitLimit, TimeSpan Window);

public sealed record SecurityRateLimitPolicy(
    RateLimitBucket Ip,
    RateLimitBucket? User = null,
    RateLimitBucket? Provider = null);

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
                new RateLimitBucket(20, TimeSpan.FromMinutes(15))),
            [RateLimitPolicyNames.AuthenticatedQuery] = new(
                new RateLimitBucket(300, TimeSpan.FromMinutes(1)),
                new RateLimitBucket(100, TimeSpan.FromMinutes(1))),
            [RateLimitPolicyNames.FinancialQuery] = new(
                new RateLimitBucket(120, TimeSpan.FromMinutes(1)),
                new RateLimitBucket(60, TimeSpan.FromMinutes(1))),
            [RateLimitPolicyNames.StandardMutation] = new(
                new RateLimitBucket(60, TimeSpan.FromMinutes(1)),
                new RateLimitBucket(20, TimeSpan.FromMinutes(1))),
            [RateLimitPolicyNames.SensitiveMutation] = new(
                new RateLimitBucket(30, TimeSpan.FromMinutes(1)),
                new RateLimitBucket(10, TimeSpan.FromMinutes(1))),
            [RateLimitPolicyNames.FinancialMutation] = new(
                new RateLimitBucket(15, TimeSpan.FromMinutes(1)),
                new RateLimitBucket(5, TimeSpan.FromMinutes(1))),
            [RateLimitPolicyNames.AdminFinancialMutation] = new(
                new RateLimitBucket(10, TimeSpan.FromMinutes(1)),
                new RateLimitBucket(3, TimeSpan.FromMinutes(1))),
            [RateLimitPolicyNames.PaymentWebhook] = new(
                new RateLimitBucket(120, TimeSpan.FromMinutes(1)),
                Provider: new RateLimitBucket(
                    1_000,
                    TimeSpan.FromMinutes(1)))
        };

    public static bool TryGet(string policyName, out SecurityRateLimitPolicy policy)
    {
        return Policies.TryGetValue(policyName, out policy!);
    }
}

public static class RateLimitResponse
{
    public const string Message =
        "لقد تجاوزت الحد المسموح من الطلبات. يرجى المحاولة مرة أخرى لاحقًا.";
}
