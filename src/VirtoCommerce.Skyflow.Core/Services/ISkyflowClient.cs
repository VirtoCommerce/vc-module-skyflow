using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VirtoCommerce.Skyflow.Core.Models;

namespace VirtoCommerce.Skyflow.Core.Services
{
    public interface ISkyflowClient
    {
        Task<SkyflowBearerTokenResponse> GetBearerToken();
        Task<HttpResponseMessage> InvokeConnection(string connectionName, HttpRequestMessage request);
        Task<IEnumerable<SkyflowCard>> GetCards(SkyflowStoreConfig config, string userId);
        Task<IDictionary<string, string>> GetCardTokens(SkyflowStoreConfig config, string skyflowId);
        Task<bool> DeleteCard(SkyflowStoreConfig config, string skyflowId);
    }
}
