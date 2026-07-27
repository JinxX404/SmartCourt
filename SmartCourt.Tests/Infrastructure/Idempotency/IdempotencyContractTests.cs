using SmartCourt.Common.Exceptions;
using SmartCourt.Infrastructure.Idempotency;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Idempotency;

public sealed class IdempotencyContractTests
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid ResourceId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void Header_RequiresNonEmptyKeyWithinStorageLimit()
    {
        Assert.Equal(
            "client-key",
            IdempotencyHeader.Require("  client-key  "));
        Assert.Throws<BusinessException>(
            () => IdempotencyHeader.Require(" "));
        Assert.Throws<BusinessException>(
            () => IdempotencyHeader.Require(
                new string('x', IdempotencyHeader.MaximumLength + 1)));
    }

    [Fact]
    public void CanonicalHash_IgnoresObjectPropertyInsertionOrder()
    {
        var scope = new IdempotencyScope(
            UserId,
            "fund",
            "Milestone",
            ResourceId);
        var hasher = new CanonicalIdempotencyRequestHasher();
        var first = new Dictionary<string, object?>
        {
            ["amount"] = 100m,
            ["currency"] = "EGP",
            ["reference"] = "mock-success-card"
        };
        var second = new Dictionary<string, object?>
        {
            ["reference"] = "mock-success-card",
            ["currency"] = "EGP",
            ["amount"] = 100m
        };

        Assert.Equal(
            hasher.ComputeHash(scope, first),
            hasher.ComputeHash(scope, second));
    }

    [Fact]
    public void CanonicalHash_ChangesWhenScopeOrPayloadChanges()
    {
        var scope = new IdempotencyScope(
            UserId,
            "fund",
            "Milestone",
            ResourceId);
        var otherScope = new IdempotencyScope(
            UserId,
            "release",
            IdempotencyScope.HoldSettlementResourceType,
            ResourceId);
        var hasher = new CanonicalIdempotencyRequestHasher();

        var originalHash = hasher.ComputeHash(
            scope,
            new { amount = 100m, currency = "EGP" });
        var changedPayloadHash = hasher.ComputeHash(
            scope,
            new { amount = 101m, currency = "EGP" });
        var changedScopeHash = hasher.ComputeHash(
            otherScope,
            new { amount = 100m, currency = "EGP" });

        Assert.NotEqual(originalHash, changedPayloadHash);
        Assert.NotEqual(originalHash, changedScopeHash);
        Assert.Equal(64, originalHash.Length);
    }

    [Fact]
    public void HoldSettlementScope_UsesOneBusinessResourceType()
    {
        var scope = IdempotencyScope.ForHoldSettlement(
            UserId,
            "release",
            ResourceId);

        Assert.Equal(
            IdempotencyScope.HoldSettlementResourceType,
            scope.ResourceType);
        Assert.Equal(ResourceId, scope.ResourceId);
    }
}
