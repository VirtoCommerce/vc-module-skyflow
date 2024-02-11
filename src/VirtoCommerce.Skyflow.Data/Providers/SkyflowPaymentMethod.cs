using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Skyflow.Core;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Data.Providers
{
    public class SkyflowPaymentMethod(ISkyflowClient skyflowClient) : PaymentMethod(nameof(SkyflowPaymentMethod))
    {
        public override ProcessPaymentRequestResult ProcessPayment(ProcessPaymentRequest request)
        {
            var tokenResponse = skyflowClient.GetBearerToken().Result;
            var vaultId = Settings.GetValue<string>(ModuleConstants.Settings.General.VaultId);
            var vaultUrl = Settings.GetValue<string>(ModuleConstants.Settings.General.VaultUrl);
            var tableName = Settings.GetValue<string>(ModuleConstants.Settings.General.TableName);

            var result = new ProcessPaymentRequestResult
            {
                IsSuccess = true,
                NewPaymentStatus = PaymentStatus.Pending,
                PublicParameters = new Dictionary<string, string>
                {
                    {"accessToken", tokenResponse.AccessToken},
                    {"vaultID", vaultId},
                    {"vaultURL", vaultUrl},
                    {"tableName", tableName}
                }
            };

            var payment = (PaymentIn)request.Payment;
            payment.PaymentStatus = PaymentStatus.Pending;
            payment.Status = payment.PaymentStatus.ToString();

            return result;
        }

        public override PostProcessPaymentRequestResult PostProcessPayment(PostProcessPaymentRequest request)
        {
            var url = Settings.GetValue<string>(ModuleConstants.Settings.General.ConnectionUrl);
            var body = Settings.GetValue<string>(ModuleConstants.Settings.General.ConnectionBody);
            var regex = new Regex("\\$[a-zA-Z_]+");
            body = regex.Replace(body, match => request.Parameters[match.Value.TrimStart('$')]);

            var response = skyflowClient.InvokeConnection(url, body).Result;

            return new PostProcessPaymentRequestResult
            {
                PublicParameters = new Dictionary<string, string>
                {
                    {"response", response}
                },
                IsSuccess = true
            };
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
            return new ValidatePostProcessRequestResult
            {
                IsSuccess = true
            };
        }

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.PreparedForm;
        public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.Alternative;
    }
}
