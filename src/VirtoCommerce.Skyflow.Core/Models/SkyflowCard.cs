using System.Collections.Generic;
using System.Text.Json.Serialization;
using VirtoCommerce.Platform.Core.Common;

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

        [JsonPropertyName("inactive")]
        public bool Inactive
        {
            get
            {
                return CheckInactive();
            }
        }

        private bool CheckInactive()
        {
            var month = 0;
            var year = 0;
            if (!CardExpiration.IsNullOrEmpty())
            {
                // format MM/YY
                var parts = CardExpiration.Split('/');
                if (parts.Length == 2)
                {
                    month = int.Parse(parts[0]);
                    year = int.Parse(parts[1]);
                }
            }

            if (!ExpiryMonth.IsNullOrEmpty())
            {
                month = int.Parse(ExpiryMonth);
            }
            if (!ExpiryYear.IsNullOrEmpty())
            {
                year = int.Parse(ExpiryYear);
            }

            if (year != 0 && month != 0)
            {
                // 08/20

                if (year < 100)
                {
                    year += 2000;
                }

                var now = System.DateTime.UtcNow;
                return year < now.Year || (year == now.Year && month < now.Month);
            }

            return false;
        }
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
