using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.PaymentModule.Core.Model.Search;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Skyflow.Core;
using VirtoCommerce.Skyflow.Core.Models;
using VirtoCommerce.Skyflow.Data.Providers;

namespace VirtoCommerce.Skyflow.Data.Extensions;

public static class PaymentMethodSearchExtensions
{
    public static async Task<SkyflowStoreConfig> GetSettingsAsync(this IPaymentMethodsSearchService paymentMethods, ISettingsManager settingsManager, string storeId)
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
        var result = settings.GetSkyflowStoreConfig();

        return result;
    }

    public static SkyflowStoreConfig GetSkyflowStoreConfig(this ICollection<ObjectSettingEntry> settings)
    {
        var result = new SkyflowStoreConfig
        {
            VaultId = GetSetting(settings, ModuleConstants.Settings.General.VaultId),
            VaultUrl = GetSetting(settings, ModuleConstants.Settings.General.VaultUrl),
            TableName = GetSetting(settings, ModuleConstants.Settings.General.TableName)
        };

        return result;
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
