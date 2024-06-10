using System.ComponentModel.DataAnnotations;

namespace VirtoCommerce.Skyflow.Core.Models
{
    public class SkyflowOptions
    {
        /// <summary>
        ///  url uses for receiving auth token to consume SkyFlow api. (can be taken from credentials.json file download from Skyflow dashboard)
        /// </summary>
        /// <example>
        /// https://manage.skyflowapis-preview.com/v1/auth/sa/oauth/token
        /// </example>
        [Required]
        public string TokenUri { get; set; }
        /// <summary>
        /// vault uri 
        /// </summary>
        /// <example>
        /// https://a370a9658141.vault.skyflowapis-preview.com
        /// </example>
        [Required]
        public string VaultUri { get; set; }
        /// <summary>
        ///  URI for invoke outbound connection rules. can be taken from skyflow studio from the connection view)
        /// </summary>
        /// <example>
        /// https://a370a9658141.gateway.skyflowapis-preview.com
        /// </example>
        [Required]
        public string GatewayUri { get; set; }
        /// <summary>
        /// Vault identifier.  can be taken from skyflow studio
        /// </summary>
        /// <example>
        /// ff9fc275bec848318361cc8928e094d1
        /// </example>
        [Required]
        public string VaultId { get; set; }
        /// <summary>
        /// Table name where credit cards are stored
        /// </summary>
        [Required]
        public string TableName { get; set; } = "credit_cards";
        /// <summary>
        /// Service account with limited permissions only for create and tokenization cards records. Used by PaymentForm.
        /// </summary>
        [Required]
        public SkyFlowServiceAccountOptions PaymentFormAccount { get; set; }
        /// <summary>
        /// Service account with limited permissions only for execute connections and read the redacted cards data from vault. Used by concrete payment methods.
        /// </summary>
        [Required]
        public SkyFlowServiceAccountOptions IntegrationsAccount { get; set; }

        /// <summary>
        /// Payment method type name that will be used for payment processing on behalf of skyflow
        /// </summary>
        public string TargetPaymentMethod { get; set; }
        /// <summary>
        /// "{GatewayUri}/v1/gateway/outboundRoutes/{TargetConnectionRoute}
        /// </summary>
        public string TargetConnectionRoute { get; set; }
    }

    public class SkyFlowServiceAccountOptions
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string KeyId { get; set; }
        public string PrivateKey { get; set; }
    }
}
