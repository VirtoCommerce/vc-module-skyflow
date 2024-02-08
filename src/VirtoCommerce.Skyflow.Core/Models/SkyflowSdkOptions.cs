namespace VirtoCommerce.Skyflow.Core.Models
{
    public class SkyflowSdkOptions
    {
        public const string ClientSdkSettingName = "ClientSDK";
        public const string ServerSdkSettingName = "ServerSDK";

        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string TokenUri { get; set; }
        public string KeyId { get; set; }
        public string PrivateKey { get; set; }
    }
}
