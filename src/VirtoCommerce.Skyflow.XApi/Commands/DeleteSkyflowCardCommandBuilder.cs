using GraphQL;
using GraphQL.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;

namespace VirtoCommerce.Skyflow.XApi.Commands;

public class DeleteSkyflowCardCommandBuilder(IMediator mediator,
        IAuthorizationService authorizationService
        //ISkyflowClient skyflowClient,
        //ISettingsManager settingsManager,
        //IPaymentMethodsSearchService paymentMethods
        ) : CommandBuilder<DeleteSkyflowCardCommand, bool, DeleteSkyflowCardCommandType, BooleanGraphType>(mediator, authorizationService)
{
    protected override string Name => "DeleteSkyflowCard";

    protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, DeleteSkyflowCardCommand request)
    {
        // Skyflow doesn't return user_id in the response, so we can't check if the user is the owner of the card
        //var userId = context.GetCurrentUserId();

        //var config = await paymentMethods.GetSettingsAsync(settingsManager, request.StoreId);

        //var card = await skyflowClient.GetCardTokens(config, request.SkyflowId);

        //if (card["user_id"] != userId)
        //{
        //    throw context.IsAuthenticated()
        //        ? AuthorizationError.Forbidden()
        //        : AuthorizationError.AnonymousAccessDenied();
        //}
        await base.BeforeMediatorSend(context, request);
    }

    protected override DeleteSkyflowCardCommand GetRequest(IResolveFieldContext<object> context)
    {
        var result = base.GetRequest(context);
        result.UserId = context.GetCurrentUserId();
        return result;
    }
}
