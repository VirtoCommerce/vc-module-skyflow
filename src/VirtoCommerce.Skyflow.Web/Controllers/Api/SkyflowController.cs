using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Skyflow.Core;

namespace VirtoCommerce.Skyflow.Web.Controllers.Api
{
    [Route("api/skyflow")]
    public class SkyflowController : Controller
    {
        // GET: api/skyflow
        /// <summary>
        /// Get message
        /// </summary>
        /// <remarks>Return "Hello world!" message</remarks>
        [HttpGet]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public ActionResult<string> Get()
        {
            return Ok(new { result = "Hello world!" });
        }
    }
}
