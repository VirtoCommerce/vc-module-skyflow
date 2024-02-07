using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Web.Controllers.Api
{
    [Route("api/skyflow")]
    public class SkyflowController(ISkyflowClient client) : ControllerBase
    {
        [HttpGet]
        [Route("token")]
        public async Task<ActionResult> GetToken()
        {
            var result = await client.GetBearerToken();
            return Ok(result);
        }
    }
}
