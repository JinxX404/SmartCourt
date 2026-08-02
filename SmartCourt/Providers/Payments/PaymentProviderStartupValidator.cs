using SmartCourt.Infrastructure.Providers.Payments;

namespace SmartCourt.Providers.Payments;

public sealed class PaymentProviderStartupValidator(
    IEnumerable<IPaymentProvider> paymentProviders,
    IEnumerable<IPaymentReconciliationProvider> reconciliationProviders)
    : IPaymentProviderStartupValidator
{
    private readonly IReadOnlyCollection<IPaymentProvider>
        _paymentProviders = paymentProviders.ToArray();
    private readonly IReadOnlyCollection<IPaymentReconciliationProvider>
        _reconciliationProviders = reconciliationProviders.ToArray();

    public void Validate()
    {
        if (_paymentProviders.Count != 1)
        {
            throw new InvalidOperationException(
                "يجب تسجيل مزود دفع تشغيلي واحد فقط قبل بدء التطبيق.");
        }

        if (_reconciliationProviders.Count != 1)
        {
            throw new InvalidOperationException(
                "يجب تسجيل مزود مطابقة دفع واحد يدعم الإيداع والتحرير والاسترداد والسحب قبل بدء التطبيق.");
        }

        if (!ReferenceEquals(
                _paymentProviders.Single(),
                _reconciliationProviders.Single()))
        {
            throw new InvalidOperationException(
                "يجب أن تستخدم عمليات الدفع والمطابقة نسخة المزود نفسها داخل نطاق الطلب.");
        }
    }
}
