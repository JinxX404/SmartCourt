using System.Reflection;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Providers;

public sealed class ProviderContractTests
{
    [Fact]
    public void PaymentProvider_ExposesAllAsyncOperationsWithCancellation()
    {
        var methods = typeof(IPaymentProvider).GetMethods();

        Assert.Equal(
            new[]
            {
                "DepositAsync",
                "RefundAsync",
                "ReleaseAsync",
                "RetryDepositAsync",
                "WithdrawAsync"
            },
            methods.Select(method => method.Name).OrderBy(name => name));

        Assert.All(
            methods,
            method =>
            {
                Assert.Equal(typeof(Task<ProviderResult>), method.ReturnType);
                Assert.Contains(
                    method.GetParameters(),
                    parameter => parameter.ParameterType == typeof(CancellationToken));
                Assert.Equal(
                    typeof(ProviderResult),
                    method.ReturnType.GetGenericArguments().Single());
            });
    }

    [Fact]
    public void ReconciliationProvider_RequiresEveryFinancialStatusCapability()
    {
        var methods = typeof(IPaymentReconciliationProvider).GetMethods();

        Assert.Equal(
            new[]
            {
                "GetDepositStatusAsync",
                "GetRefundStatusAsync",
                "GetReleaseStatusAsync",
                "GetWithdrawalStatusAsync"
            },
            methods.Select(method => method.Name).OrderBy(name => name));
        Assert.All(
            methods,
            method =>
            {
                Assert.True(method.IsAbstract);
                Assert.Equal(
                    typeof(Task<ProviderResult>),
                    method.ReturnType);
                Assert.Contains(
                    method.GetParameters(),
                    parameter => parameter.ParameterType
                        == typeof(CancellationToken));
            });
    }

    [Fact]
    public void ProviderRequestsAndResult_CarryFinancialCorrelationContract()
    {
        var commonProperties = new[]
        {
            nameof(PaymentProviderRequest.Amount),
            nameof(PaymentProviderRequest.Currency),
            nameof(PaymentProviderRequest.BusinessId),
            nameof(PaymentProviderRequest.ProviderIdempotencyKey),
            nameof(PaymentProviderRequest.CorrelationId)
        };

        Assert.Equal(
            commonProperties,
            typeof(PaymentProviderRequest)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.Name)
                .ToArray());

        Assert.Equal(
            commonProperties
                .Append(nameof(ProviderResult.Outcome))
                .Append(nameof(ProviderResult.ProviderTransactionId))
                .Append(nameof(ProviderResult.FailureReason))
                .Append(nameof(ProviderResult.ProviderStatus))
                .Append(nameof(ProviderResult.ProviderObjectType))
                .Append(nameof(ProviderResult.ProviderMoney))
                .Append(nameof(ProviderResult.ClientAction))
                .Append(nameof(ProviderResult.RelatedProviderTransactionId)),
            typeof(ProviderResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.Name));
    }

    [Fact]
    public void ProviderOutcomes_DistinguishSuccessFailureAndUnknown()
    {
        Assert.Equal(
            new[]
            {
                "Succeeded",
                "Failed",
                "Unknown",
                "Processing",
                "RequiresCustomerAction"
            },
            Enum.GetNames<ProviderOperationOutcome>());
    }

    [Fact]
    public void JobSchedulerAndOutboxDispatcher_AreAsyncAndCancellable()
    {
        Assert.All(
            typeof(IContractJobScheduler).GetMethods(),
            method => AssertAsyncAndCancellable(method, typeof(string)));

        Assert.All(
            typeof(IOutboxDispatcher).GetMethods(),
            method =>
            {
                var expectedResult = method.Name == nameof(IOutboxDispatcher.DispatchAsync)
                    ? typeof(Task)
                    : typeof(Task<int>);
                Assert.Equal(expectedResult, method.ReturnType);
                Assert.Contains(
                    method.GetParameters(),
                    parameter => parameter.ParameterType == typeof(CancellationToken));
            });
    }

    private static void AssertAsyncAndCancellable(
        MethodInfo method,
        Type resultType)
    {
        Assert.Equal(typeof(Task<>).MakeGenericType(resultType), method.ReturnType);
        Assert.Contains(
            method.GetParameters(),
            parameter => parameter.ParameterType == typeof(CancellationToken));
    }
}
