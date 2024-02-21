using VirtoCommerce.PaymentModule.Model.Requests;

namespace VirtoCommerce.Skyflow.Core.Services
{
    public interface IPaymentClientFactory
    {
        IPaymentClient GetPaymentClient(PaymentRequestBase request);
        string GetConnectionName(PaymentRequestBase request);
    }
}
