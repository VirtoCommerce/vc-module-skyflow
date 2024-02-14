using System.Collections.Generic;

namespace VirtoCommerce.Skyflow.Core.Models
{
    public class SkyflowOptions
    {
        public string TokenUri { get; set; }
        public SkyflowSdkOptions ClientSdk { get; set; }
        public IDictionary<string, SkyflowSdkOptions> Connections { get; set; }
    }
}
