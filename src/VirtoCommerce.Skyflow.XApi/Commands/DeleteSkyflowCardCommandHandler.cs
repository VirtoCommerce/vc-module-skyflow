using MediatR;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Skyflow.Core.Services;
using VirtoCommerce.Skyflow.Data.Extensions;

namespace VirtoCommerce.Skyflow.XApi.Commands;

public class DeleteSkyflowCardCommandHandler(
    ISkyflowClient skyflowClient,
    ISettingsManager settingsManager,
    IPaymentMethodsSearchService paymentMethods) : IRequestHandler<DeleteSkyflowCardCommand, bool>
{
    public async Task<bool> Handle(DeleteSkyflowCardCommand request, CancellationToken cancellationToken)
    {
        var config = await paymentMethods.GetSettingsAsync(settingsManager, request.StoreId);
        var result = await skyflowClient.DeleteCard(config, request.SkyflowId);
        return result;
    }
}
