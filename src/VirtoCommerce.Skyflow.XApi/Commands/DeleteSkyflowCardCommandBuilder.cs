using System;
using GraphQL;
using GraphQL.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Skyflow.XApi.Commands;

public class DeleteSkyflowCardCommandBuilder(IAuthorizationService authorizationService)
    : CommandBuilder<DeleteSkyflowCardCommand, bool, DeleteSkyflowCardCommandType, BooleanGraphType>(authorizationService)
{
    [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public DeleteSkyflowCardCommandBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : this(authorizationService)
    {
    }

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
