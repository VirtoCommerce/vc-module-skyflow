using Microsoft.IdentityModel.Tokens;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.PaymentModule.Core.Model.Search;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Skyflow.Core;
using VirtoCommerce.Skyflow.Core.Services;
using VirtoCommerce.Skyflow.Data.Providers;

namespace VirtoCommerce.Skyflow.XApi.Queries;

public class SkyflowCardQueryHandler(
    ISkyflowClient skyflowClient,
    ISettingsManager settingsManager,
    IPaymentMethodsSearchService paymentMethods) : IQueryHandler<SkyflowCardQuery, SkyflowCardResponse>
{
    public async Task<SkyflowCardResponse> Handle(SkyflowCardQuery request, CancellationToken cancellationToken)
    {
        if (request.StoreId.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(request.StoreId));
        }
        var paymentMethodSearchResult = await paymentMethods.SearchAsync(new PaymentMethodsSearchCriteria { Keyword = nameof(SkyflowPaymentMethod), StoreId = request.StoreId });
        var paymentMethod = paymentMethodSearchResult.Results.FirstOrDefault();

        if (paymentMethod == null)
        {
            throw new InvalidOperationException("Skyflow payment method not found");
        }

        await settingsManager.DeepLoadSettingsAsync(paymentMethod);

        var settings = paymentMethod.Settings;

        var vaultId = settings.GetValue<string>(ModuleConstants.Settings.General.VaultId);
        if (vaultId.IsNullOrEmpty())
        {
            throw new InvalidOperationException("VaultId is not set");
        }
        var vaultUrl = settings.GetValue<string>(ModuleConstants.Settings.General.VaultUrl);
        if (vaultUrl.IsNullOrEmpty())
        {
            throw new InvalidOperationException("VaultUrl is not set");
        }
        var tableName = settings.GetValue<string>(ModuleConstants.Settings.General.TableName);

        var cards = await skyflowClient.GetCards(vaultUrl, vaultId, tableName, request.UserId);

        return new SkyflowCardResponse
        {
            Cards = cards
        };
    }
}
