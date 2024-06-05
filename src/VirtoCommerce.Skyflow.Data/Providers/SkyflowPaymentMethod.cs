using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Data.Providers
{
    public class SkyflowPaymentMethod: PaymentMethod
    {
        private readonly SkyflowOptions _options;
        private readonly ISkyflowClient _skyFlowClient;
        private readonly IPaymentMethodsRegistrar _paymentMethodsRegistrar;
        public SkyflowPaymentMethod(
            ISkyflowClient skyflowClient,
            IPaymentMethodsRegistrar paymentMethodsRegistrar,
            IOptions<SkyflowOptions> options)
            :base(nameof(SkyflowPaymentMethod))
        {
            _skyFlowClient = skyflowClient;
            _options = options.Value;
            _paymentMethodsRegistrar = paymentMethodsRegistrar;
        }
   
        public override ProcessPaymentRequestResult ProcessPayment(ProcessPaymentRequest request)
        {
            var tokenResponse = _skyFlowClient.GetBearerToken(_options.PaymentFormAccount).GetAwaiter().GetResult();

            var result = new ProcessPaymentRequestResult
            {
                IsSuccess = true,
                NewPaymentStatus = PaymentStatus.Pending,
                PublicParameters = new Dictionary<string, string>
                {
                    {"accessToken", tokenResponse.AccessToken},
                    {"vaultID", _options.VaultId},
                    {"vaultURL", $"{_options.VaultUri}"},
                    {"tableName", _options.TableName}
                }
            };
            var payment = (PaymentIn)request.Payment;
            payment.PaymentStatus = PaymentStatus.Pending;
            payment.Status = payment.PaymentStatus.ToString();

            return result;
        }

        public override PostProcessPaymentRequestResult PostProcessPayment(PostProcessPaymentRequest request)
        {
            return PostProcessPaymentAsync(request).GetAwaiter().GetResult();
        }

        protected virtual async Task<PaymentMethod> GetDecoratedPaymentMethod(PostProcessPaymentRequest request)
        {
            var paymentMethod = (await _paymentMethodsRegistrar.GetRegisteredPaymentMethods()).FirstOrDefault(x => x.TypeName.EqualsInvariant(_options.DefaultPaymentMethod));
            if (paymentMethod == null)
            {
                throw new OperationCanceledException("Payment method not found. Please check the Payments:Skyflow:DefaultPaymentMethod setting");
            }
            return paymentMethod;
        }
        private async Task<PostProcessPaymentRequestResult> PostProcessPaymentAsync(PostProcessPaymentRequest request)
        {
            var paymentMethod = await GetDecoratedPaymentMethod(request);

            var skyflowId = request.Parameters["skyflow_id"];
            if (string.IsNullOrEmpty(skyflowId))
            {
                throw new InvalidOperationException("Skyflow ID is required");
            }

            var order = (CustomerOrder)request.Order;
            var userId = order.CustomerId;

            var skyFlowCard = await _skyFlowClient.GetCard(skyflowId);
            if (skyFlowCard == null)
            {
                throw new OperationCanceledException($"SkyFlow card record not found");
            }
            if (!string.IsNullOrEmpty(skyFlowCard.UserId) && skyFlowCard.UserId != order.CustomerId)
            {
                throw new UnauthorizedAccessException($"Payment cannot be processed using a card registered to another user");
            }
            request.Parameters["CreditCard"] = JsonConvert.SerializeObject(skyFlowCard);
            request.Parameters["ProxyHttpClientName"] = "SkyFlow";
            request.Parameters["ProxyEndpointUrl"] = new Uri($"{_options.GatewayUri}/v1/gateway/outboundRoutes/{_options.DefaultConnectionRoute}").ToString();
            var result = paymentMethod.PostProcessPayment(request);
            return result;
        }

        public override VoidPaymentRequestResult VoidProcessPayment(VoidPaymentRequest request)
        {
            throw new NotImplementedException();
        }

        public override CapturePaymentRequestResult CaptureProcessPayment(CapturePaymentRequest context)
        {
            return new CapturePaymentRequestResult { IsSuccess = true, NewPaymentStatus = PaymentStatus.Paid };
        }

        public override RefundPaymentRequestResult RefundProcessPayment(RefundPaymentRequest context)
        {
            return new RefundPaymentRequestResult { IsSuccess = true, NewRefundStatus = RefundStatus.Processed };
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
