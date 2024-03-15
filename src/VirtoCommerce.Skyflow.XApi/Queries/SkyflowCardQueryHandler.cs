using Microsoft.IdentityModel.Tokens;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Skyflow.Core.Services;
using VirtoCommerce.Skyflow.Data.Extensions;

namespace VirtoCommerce.Skyflow.XApi.Queries;

public class SkyflowCardQueryHandler(
    ISkyflowClient skyflowClient,
    ISettingsManager settingsManager,
    IPaymentMethodsSearchService paymentMethods) : IQueryHandler<SkyflowCardQuery, SkyflowCardResponse>
{
    public async Task<SkyflowCardResponse> Handle(SkyflowCardQuery request, CancellationToken cancellationToken)
    {
        CheckStoreId(request.StoreId);

        var config = await paymentMethods.GetSettingsAsync(settingsManager, request.StoreId);
        var cards = await skyflowClient.GetCards(config, request.UserId);

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
}
