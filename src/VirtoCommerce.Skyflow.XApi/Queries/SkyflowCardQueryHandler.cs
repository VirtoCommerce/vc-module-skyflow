using Microsoft.IdentityModel.Tokens;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.XApi.Queries;

public class SkyflowCardQueryHandler(
    ISkyflowClient skyflowClient) : IQueryHandler<SkyflowCardQuery, SkyflowCardResponse>
{
    public async Task<SkyflowCardResponse> Handle(SkyflowCardQuery request, CancellationToken cancellationToken)
    {
        CheckStoreId(request.StoreId);

        var cards = await skyflowClient.GetAllSavedUserCards(request.UserId);

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
