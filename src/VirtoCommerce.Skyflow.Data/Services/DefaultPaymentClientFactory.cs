using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Data.Services
{
    public class DefaultPaymentClientFactory(IEnumerable<IPaymentClient> clients) : IPaymentClientFactory
    {
        public virtual IPaymentClient GetPaymentClient(PaymentRequestBase request)
        {
            return clients.FirstOrDefault();
        }

        public virtual string GetConnectionName(PaymentRequestBase request)
        {
            return "Default";
        }
    }
}
