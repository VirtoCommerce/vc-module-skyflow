using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VirtoCommerce.Skyflow.Core.Models
{
    public class SkyflowCard
    {
        [JsonPropertyName("card_expiration")]
        public string CardExpiration { get; set; }

        [JsonPropertyName("card_number")]
        public string CardNumber { get; set; }

        [JsonPropertyName("cardholder_name")]
        public string CardholderName { get; set; }

        [JsonPropertyName("cvv")]
        public string Cvv { get; set; }

        [JsonPropertyName("expiry_month")]
        public string ExpiryMonth { get; set; }

        [JsonPropertyName("expiry_year")]
        public string ExpiryYear { get; set; }

        [JsonPropertyName("skyflow_id")]
        public string SkyflowId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }
    }

    public class SkyflowItemModel
    {
        public SkyflowCard Fields { get; set; }
    }

    public class SkyflowResponseModel
    {
        public IEnumerable<SkyflowItemModel> Records { get; set; }
    }

    public class SkyflowTableRowModel
    {
        public IDictionary<string, string> Fields { get; set; }
    }

    public class SkyflowDeleteResponseModel
    {
        [JsonPropertyName("skyflow_id")]
        public string SkyflowId { get; set; }
        public bool Deleted { get; set; }
    }
}
