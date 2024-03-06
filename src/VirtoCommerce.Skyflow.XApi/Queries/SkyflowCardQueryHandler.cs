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
        CheckStoreId(request.StoreId);
        var settings = await GetSettingsAsync(request.StoreId);

        var vaultId = GetSetting(settings, ModuleConstants.Settings.General.VaultId);
        var vaultUrl = GetSetting(settings, ModuleConstants.Settings.General.VaultUrl);
        var tableName = GetSetting(settings, ModuleConstants.Settings.General.TableName, throwIfNull: false);

        var cards = await skyflowClient.GetCards(vaultUrl, vaultId, tableName, request.UserId);

        return new SkyflowCardResponse
        {
            Cards = cards
        };
    }

    private static void CheckStoreId(string storeId)
    {
        if (storeId.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(storeId));
        }
    }

    private async Task<ICollection<ObjectSettingEntry>> GetSettingsAsync(string storeId)
    {
        var paymentMethodSearchResult = await paymentMethods.SearchAsync(new PaymentMethodsSearchCriteria
        {
            Keyword = nameof(SkyflowPaymentMethod),
            StoreId = storeId,
            Codes = { nameof(SkyflowPaymentMethod) },
            IsActive = true
        });
        var paymentMethod = paymentMethodSearchResult.Results.FirstOrDefault();

        if (paymentMethod == null)
        {
            throw new InvalidOperationException("Skyflow payment method not found");
        }

        await settingsManager.DeepLoadSettingsAsync(paymentMethod);

        var settings = paymentMethod.Settings;
        return settings;
    }

    private static string GetSetting(ICollection<ObjectSettingEntry> settings, SettingDescriptor descriptor, bool throwIfNull = true)
    {
        var result = settings.GetValue<string>(descriptor);
        if (throwIfNull && result.IsNullOrEmpty())
        {
            throw new InvalidOperationException($"Setting {descriptor.Name} is not set");
        }

        return result;
    }
}
