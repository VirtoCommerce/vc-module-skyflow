using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Web.Controllers.Api
{
    [Route("api/skyflow")]
    public class SkyflowController(
        ISkyflowClient client,
        IOptionsSnapshot<SkyflowSdkOptions> optionsAccessor
    ) : ControllerBase
    {
        [HttpGet]
        [Route("token")]
        public async Task<ActionResult> GetToken()
        {
            var result = await client.GetBearerToken();
            return Ok(result);
        }

        [HttpGet]
        [Route("vault")]
        public ActionResult GetVault(string name)
        {
            if (name == "client-sdk")
            {
                return Ok(optionsAccessor.Get(SkyflowSdkOptions.ClientSdkSettingName));
            }
            if (name == "server-sdk")
            {
                return Ok(optionsAccessor.Get(SkyflowSdkOptions.ServerSdkSettingName));
            }
            return Ok();
        }
    }
}
