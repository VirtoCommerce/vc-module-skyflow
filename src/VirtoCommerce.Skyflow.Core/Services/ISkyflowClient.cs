using System.Threading.Tasks;
using VirtoCommerce.Skyflow.Core.Models;

namespace VirtoCommerce.Skyflow.Core.Services
{
    public interface ISkyflowClient
    {
        Task<SkyflowBearerTokenResponse> GetBearerToken();
        Task<string> InvokeConnection(string url, string body);
    }
}
