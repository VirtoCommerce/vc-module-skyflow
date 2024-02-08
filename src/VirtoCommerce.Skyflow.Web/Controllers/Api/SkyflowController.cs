using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Skyflow.Core;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Core.Services;
using VirtoCommerce.Skyflow.Data.Providers;

namespace VirtoCommerce.Skyflow.Web.Controllers.Api
{
    [Route("api/skyflow")]
    public class SkyflowController(
        ISkyflowClient client,
        ISettingsManager settings,
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
            return Ok(new
            {
                vaultId = settings.GetValue<string>(ModuleConstants.Settings.General.VaultId),
                vaultUrl = settings.GetValue<string>(ModuleConstants.Settings.General.VaultUrl),
                tableName = settings.GetValue<string>(ModuleConstants.Settings.General.TableName)
            });
        }

        [HttpGet]
        [Route("initializePayment")]
        public ActionResult GetProcessPayment()
        {
            var paymentMethod = new SkyflowPaymentMethod(client);
            var response = paymentMethod.ProcessPayment(new ProcessPaymentRequest
            {
                Payment = new PaymentIn()
            });

            return Ok(response);
        }
    }
}
