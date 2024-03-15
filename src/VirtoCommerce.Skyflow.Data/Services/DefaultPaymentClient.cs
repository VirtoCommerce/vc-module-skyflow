using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
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

        private const string AuthBodyTemplate = """
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
            var sectionConfig = configuration.GetSection("Payments:Skyflow:DefaultConnection").Get<DefaultConnectionOptions>();

            var sum = order.Total;
            var currency = order.Currency;

            var body = ReplaceVariableRegex().Replace(AuthBodyTemplate, match => match.Value switch
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

        public PostProcessPaymentRequestResult CreatePostProcessPaymentResponse(PaymentRequestBase request, HttpResponseMessage responseMessage)
        {
            var responseText = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var doc = XDocument.Parse(responseText);

            var ns = doc.Root?.GetDefaultNamespace();
            var nsManager = new XmlNamespaceManager(new NameTable());
            if (ns != null)
            {
                nsManager.AddNamespace("ns", ns.NamespaceName);
            }

            var element = doc.XPathSelectElement("//ns:createTransactionResponse/ns:messages/ns:resultCode", nsManager);

            if (element != null)
            {
                var resultCode = element.Value;
                if (resultCode == "Ok")
                {
                    return CreateSuccessResult(request, responseText);
                }

                if (resultCode == "Error")
                {
                    var messageText = doc.XPathSelectElement("//ns:createTransactionResponse/ns:messages/ns:message/ns:text", nsManager)?.Value;
                    var errorText = doc.XPathSelectElement("//ns:createTransactionResponse/ns:transactionResponse/ns:errors/ns:errorText", nsManager)?.Value;
                    var message = $"{messageText} ({errorText})";
                    return CreateErrorResult(request, responseText, message);
                }
            }

            return CreateErrorResult(request, responseText, "Unexpected response");
        }

        public string[] RequiredParameters { get; } = { "card_number", "card_expiration", "cvv" };

        private static PostProcessPaymentRequestResult CreateErrorResult(PaymentRequestBase request, string responseText, string message)
        {
            var payment = (PaymentIn)request.Payment;

            payment.Status = PaymentStatus.Error.ToString();
            payment.ProcessPaymentResult = new ProcessPaymentRequestResult
            {
                ErrorMessage = $"There was an error processing your transaction: {message}",
            };
            payment.Comment = $"{responseText}{Environment.NewLine}";

            return new PostProcessPaymentRequestResult { ErrorMessage = payment.ProcessPaymentResult.ErrorMessage };
        }

        private static PostProcessPaymentRequestResult CreateSuccessResult(PaymentRequestBase request, string responseText)
        {
            var result = new PostProcessPaymentRequestResult { IsSuccess = true };

            var payment = (PaymentIn)request.Payment;
            var order = (CustomerOrder)request.Order;

            result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Paid;
            payment.Status = payment.PaymentStatus.ToString();
            payment.IsApproved = true;
            payment.CapturedDate = DateTime.UtcNow;
            payment.Comment = $"Paid successfully. Transaction Info {responseText}{Environment.NewLine}";

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
