using GraphQL;
using GraphQL.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Skyflow.XApi.Commands;

public class DeleteSkyflowCardCommandBuilder(IMediator mediator,
        IAuthorizationService authorizationService
        ) : CommandBuilder<DeleteSkyflowCardCommand, bool, DeleteSkyflowCardCommandType, BooleanGraphType>(mediator, authorizationService)
{
    protected override string Name => "DeleteSkyflowCard";

    protected override Task BeforeMediatorSend(IResolveFieldContext<object> context, DeleteSkyflowCardCommand request)
    {
        return base.BeforeMediatorSend(context, request);
    }

    protected override DeleteSkyflowCardCommand GetRequest(IResolveFieldContext<object> context)
    {
        var result = base.GetRequest(context);
        result.UserId = context.GetCurrentUserId();
        return result;
    }
}
