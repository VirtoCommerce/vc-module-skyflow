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

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SetHeadersAsync(request).GetAwaiter().GetResult();
            return base.Send(request, cancellationToken);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await SetHeadersAsync(request);
            return await base.SendAsync(request, cancellationToken);
        }

        private async Task SetHeadersAsync(HttpRequestMessage request)
        {
            var token = await _skyflowClient.GetBearerToken(_options.IntegrationsAccount);
            request.Headers.Add("X-Skyflow-Authorization", token.AccessToken);
            request.Headers.Add("User-Agent", "VirtoCommerce/1.0");
        }
    }
}
