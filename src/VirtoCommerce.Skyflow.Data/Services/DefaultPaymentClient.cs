using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Data.Services
{
    public class DefaultPaymentClient(IConfiguration configuration) : IPaymentClient
    {
        private static readonly Regex ReplaceVariableRegex = new("\\$[a-zA-Z_]+", RegexOptions.Compiled);

        public HttpRequestMessage CreateConnectionRequest(PaymentRequestBase request)
        {
            var order = (CustomerOrder)request.Order;
            var sectionConfig = configuration.GetSection("Skyflow:DefaultConnection").Get<DefaultConnectionOptions>();

            var sum = order.Total;
            var currency = order.Currency;
            var body = GetBodyTemplate();

            body = ReplaceVariableRegex.Replace(body, match => match.Value switch
            {
                "$name" => sectionConfig.Name,
                "$transactionKey" => sectionConfig.TransactionKey,
                "$currency" => currency,
                "$amount" => sum.ToString(CultureInfo.InvariantCulture),
                _ => request.Parameters[match.Value.TrimStart('$')],
            });

            return new HttpRequestMessage(HttpMethod.Post, sectionConfig.ConnectionUrl)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/xml")
            };
        }

        private string GetBodyTemplate()
        {
            return """
                   <createTransactionRequest xmlns="AnetApi/xml/v1/schema/AnetApiSchema.xsd">
                       <merchantAuthentication>
                           <name>$name</name>
                           <transactionKey>$transactionKey</transactionKey>
                       </merchantAuthentication>
                       <refId>123456</refId>
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
                       </transactionRequest>
                   </createTransactionRequest>
                   """;
        }

        public PostProcessPaymentRequestResult CreatePostProcessPaymentResponse(PaymentRequestBase request,
            HttpResponseMessage responseMessage)
        {
            var responseText = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return new PostProcessPaymentRequestResult
            {
                PublicParameters = new Dictionary<string, string>
                {
                    {"response", responseText}
                },
                IsSuccess = true
            };
        }

        private class DefaultConnectionOptions
        {
            public string ConnectionUrl { get; set; }
            public string Name { get; set; }
            public string TransactionKey { get; set; }
        }
    }
}
