using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.Skyflow.Core.Services
{
    public interface ISkyflowClient
    {
        Task<Dictionary<string, string>> GetBearerToken();
    }
}
