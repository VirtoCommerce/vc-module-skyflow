using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Data.Services
{
    public partial class DefaultPaymentClient(IConfiguration configuration) : IPaymentClient
    {
        [GeneratedRegex("\\$[a-z0-9_]+", RegexOptions.IgnoreCase)]
        private static partial Regex ReplaceVariableRegex();

        private const string BodyTemplate = """
                                            <createTransactionRequest xmlns="AnetApi/xml/v1/schema/AnetApiSchema.xsd">
                                                <merchantAuthentication>
                                                    <name>$name</name>
                                                    <transactionKey>$transactionKey</transactionKey>
                                                </merchantAuthentication>
                                                <transactionRequest>
                                                    <transactionType>authCaptureTransaction</transactionType>
                                                    <amount>$amount</amount>
                                                    <currencyCode>$currency</currencyCode>
                                                    <payment>
                                                        <creditCard>
                                                            <cardNumber>$card_number</cardNumber>
                                                            <expirationDate>$card_expiration</expirationDate>
                                                            <cardCode>$cvv</cardCode>
                                                        </creditCard>
                                                    </payment>
                                                    <order>
                                                        <invoiceNumber>$orderNumber</invoiceNumber>
                                                    </order>
                                                </transactionRequest>
                                            </createTransactionRequest>
                                            """;

        public HttpRequestMessage CreateConnectionRequest(PaymentRequestBase request)
        {
            var order = (CustomerOrder)request.Order;
            var sectionConfig = configuration.GetSection("Skyflow:DefaultConnection").Get<DefaultConnectionOptions>();

            var sum = order.Total;
            var currency = order.Currency;
            var body = BodyTemplate;

            body = ReplaceVariableRegex().Replace(body, match => match.Value switch
            {
                "$name" => sectionConfig.Name,
                "$transactionKey" => sectionConfig.TransactionKey,
                "$currency" => currency,
                "$amount" => sum.ToString(CultureInfo.InvariantCulture),
                "$orderNumber" => order.Number,
                _ => request.Parameters[match.Value.TrimStart('$')],
            });

            return new HttpRequestMessage(HttpMethod.Post, sectionConfig.ConnectionUrl)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/xml")
            };
        }

        public PostProcessPaymentRequestResult CreatePostProcessPaymentResponse(PaymentRequestBase request,
            HttpResponseMessage responseMessage)
        {
            var responseText = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var result = new PostProcessPaymentRequestResult
            {
                PublicParameters = new Dictionary<string, string>
                {
                    {"response", responseText}
                },

                IsSuccess = true
            };

            var transactionMessage = responseText;

            var payment = (PaymentIn)request.Payment;
            var order = (CustomerOrder)request.Order;

            result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Paid;
            payment.Status = payment.PaymentStatus.ToString();
            payment.IsApproved = true;
            payment.CapturedDate = DateTime.UtcNow;
            payment.Comment = $"Paid successfully. Transaction Info {transactionMessage}{Environment.NewLine}";

            var paymentTransaction = new PaymentGatewayTransaction
            {
                IsProcessed = true,
                ProcessedDate = DateTime.UtcNow,
                CurrencyCode = payment.Currency,
                Amount = payment.Sum,
                Note = responseText
            };

            payment.Transactions.Add(paymentTransaction);


            result.IsSuccess = true;
            result.OrderId = order.Id;

            order.Status = "Processing";
            payment.AuthorizedDate = DateTime.UtcNow;

            return result;
        }
    }
}
