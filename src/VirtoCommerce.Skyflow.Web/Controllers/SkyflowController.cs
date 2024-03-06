using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.Web.Controllers
{
    [ApiController]
    [Route("api/skyflow")]
    public class SkyflowController(ISkyflowClient skyflowClient) : Controller
    {
        [HttpGet("cards")]
        public async Task<IActionResult> GetCards(string userId)
        {
            // https://ebfc9bee4242.vault.skyflowapis.com/v1/vaults/c1aeec61ad7c46c2b724f004a7658b2f/query
            var vaultId = "c1aeec61ad7c46c2b724f004a7658b2f";// Settings.GetValue<string>(ModuleConstants.Settings.General.VaultId);
            var vaultUrl = "https://ebfc9bee4242.vault.skyflowapis.com".TrimEnd('/');// Settings.GetValue<string>(ModuleConstants.Settings.General.VaultUrl);
            var tableName = "credit_cards";// Settings.GetValue<string>(ModuleConstants.Settings.General.TableName);

            var cards = await skyflowClient.GetCards(vaultUrl, vaultId, tableName, userId);
            return Ok(cards);
        }
    }
}
