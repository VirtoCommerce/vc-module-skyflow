using MediatR;
using VirtoCommerce.Skyflow.Core.Services;

namespace VirtoCommerce.Skyflow.XApi.Commands;

public class DeleteSkyflowCardCommandHandler(
    ISkyflowClient skyflowClient) : IRequestHandler<DeleteSkyflowCardCommand, bool>
{
    public async Task<bool> Handle(DeleteSkyflowCardCommand request, CancellationToken cancellationToken)
    {
        var result = await skyflowClient.DeleteCard(request.SkyflowId);
        return result;
    }
}
