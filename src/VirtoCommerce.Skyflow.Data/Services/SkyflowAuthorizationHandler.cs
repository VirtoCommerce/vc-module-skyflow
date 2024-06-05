using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Data.Services
{
    public class SkyflowAuthorizationHandler : DelegatingHandler
    {
        private readonly ISkyflowClient _skyflowClient;
        private readonly SkyflowOptions _options;
        public SkyflowAuthorizationHandler(ISkyflowClient skyflowClient, IOptions<SkyflowOptions> options)
        {
            _options = options.Value;
            _skyflowClient = skyflowClient;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _skyflowClient.GetBearerToken(_options.IntegrationsAccount);
            // Add the Authorization header to the request
            request.Headers.Add("X-Skyflow-Authorization", $"{token.AccessToken}");
            request.Headers.Add("User-Agent", "Opus/1.0");
            // Call the inner handler
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
