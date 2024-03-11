using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Web.Controllers
{
    [ApiController]
    [Route("api/skyflow")]
    public class TestController(ISkyflowClient skyflow) : Controller
    {
        [HttpGet]
        [Route("cards")]
        public async Task<IActionResult> GetCards(string userId)
        {
            var vaultUrl = "https://ebfc9bee4242.vault.skyflowapis.com";
            var vaultId = "c1aeec61ad7c46c2b724f004a7658b2f";
            var tableName = "credit_cards";
            var result = await skyflow.GetTableData(vaultUrl, vaultId, tableName, userId);
            return Ok(result);
        }

        [HttpGet]
        [Route("card")]
        public async Task<IActionResult> GetCard(string skyflowId)
        {
            var vaultUrl = "https://ebfc9bee4242.vault.skyflowapis.com";
            var vaultId = "c1aeec61ad7c46c2b724f004a7658b2f";
            var tableName = "credit_cards";
            var result = await skyflow.GetCardTokens(vaultUrl, vaultId, tableName, skyflowId);
            return Ok(result);
        }
    }
}
