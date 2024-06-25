using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VirtoCommerce.Skyflow.Core.Models;

namespace VirtoCommerce.Skyflow.Core.Services
{
    public interface ISkyflowClient
    {
        Task<SkyflowBearerTokenResponse> GetBearerToken(SkyflowServiceAccountOptions serviceAccountOptions);
        Task<HttpResponseMessage> InvokeConnection(HttpMethod method, string route, Dictionary<string, string> headers, HttpContent content);
        Task<SkyflowCard> GetCard(string skyflowId);
        Task<IEnumerable<SkyflowCard>> GetAllSavedUserCards(string userId);
        Task<SkyflowCard[]> GetCardsByIds(string[] skyflowIds);
        Task<bool> DeleteCard(string skyflowId);
    }
}
