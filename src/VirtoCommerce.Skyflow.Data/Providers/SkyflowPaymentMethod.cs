using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Skyflow.Core;
using VirtoCommerce.Skyflow.Core.Services;
using VirtoCommerce.Skyflow.Data.Extensions;

namespace VirtoCommerce.Skyflow.Data.Providers
{
    public class SkyflowPaymentMethod(ISkyflowClient skyflowClient, IPaymentClientFactory paymentClientFactory) : PaymentMethod(nameof(SkyflowPaymentMethod))
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
            var paymentClient = paymentClientFactory.GetPaymentClient(request);
            var connectionName = paymentClientFactory.GetConnectionName(request);

            if (paymentClient.RequiredParameters.FirstOrDefault(x => request.Parameters.AllKeys.All(k => k != x)) != null)
            {
                var skyflowId = request.Parameters["skyflow_id"];
                if (string.IsNullOrEmpty(skyflowId))
                {
                    throw new InvalidOperationException("Skyflow ID is required");
                }

                var config = Settings.GetSkyflowStoreConfig();
                var tokens = skyflowClient.GetCardTokens(config, skyflowId).GetAwaiter().GetResult();

                // by some reason Skyflow doesn't return user_id. So this validation is commented out
                //var order = (CustomerOrder)request.Order;
                //var userId = order.CustomerId;

                //if (tokens["user_id"] != userId)
                //{
                //    throw new InvalidOperationException("Skyflow ID does not belong to the user");
                //}

                foreach (var key in tokens.Keys)
                {
                    request.Parameters[key] = tokens[key];
                }
            }

            using var requestMessage = paymentClient.CreateConnectionRequest(request);
            using var responseMessage = skyflowClient.InvokeConnection(connectionName, requestMessage).GetAwaiter().GetResult();

            var result = paymentClient.CreatePostProcessPaymentResponse(request, responseMessage);
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
