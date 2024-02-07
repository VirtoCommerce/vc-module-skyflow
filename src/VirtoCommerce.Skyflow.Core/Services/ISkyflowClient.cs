using System.Threading.Tasks;
using VirtoCommerce.Skyflow.Core.Models;

namespace VirtoCommerce.Skyflow.Core.Services
{
    public interface ISkyflowClient
    {
        Task<SkyflowBearerTokenResponse> GetBearerToken();
    }
}
