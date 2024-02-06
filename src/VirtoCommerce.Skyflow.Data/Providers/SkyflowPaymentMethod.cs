using System.Collections.Specialized;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;

namespace VirtoCommerce.Skyflow.Data.Providers
{
    public class SkyflowPaymentMethod : PaymentMethod
    {
        public SkyflowPaymentMethod() : base(nameof(SkyflowPaymentMethod))
        {
        }

        public override ProcessPaymentRequestResult ProcessPayment(ProcessPaymentRequest request)
        {
            return new ProcessPaymentRequestResult();
        }

        public override PostProcessPaymentRequestResult PostProcessPayment(PostProcessPaymentRequest request)
        {
            return new PostProcessPaymentRequestResult();
        }

        public override VoidPaymentRequestResult VoidProcessPayment(VoidPaymentRequest request)
        {
            return new VoidPaymentRequestResult();
        }

        public override CapturePaymentRequestResult CaptureProcessPayment(CapturePaymentRequest context)
        {
            return new CapturePaymentRequestResult();
        }

        public override RefundPaymentRequestResult RefundProcessPayment(RefundPaymentRequest context)
        {
            return new RefundPaymentRequestResult();
        }

        public override ValidatePostProcessRequestResult ValidatePostProcessRequest(NameValueCollection queryString)
        {
            return new ValidatePostProcessRequestResult();
        }

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.PreparedForm;
        public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.Alternative;
    }
}
