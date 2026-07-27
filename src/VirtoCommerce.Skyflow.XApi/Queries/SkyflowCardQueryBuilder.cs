using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Skyflow.XApi.Schemas;
using VirtoCommerce.Xapi.Core.BaseQueries;

namespace VirtoCommerce.Skyflow.XApi.Queries;

public class SkyflowCardQueryBuilder(IAuthorizationService authorizationService)
    : QueryBuilder<SkyflowCardQuery, SkyflowCardResponse, SkyflowCardResponseType>(authorizationService)
{
    [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public SkyflowCardQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : this(authorizationService)
    {
    }

    protected override string Name => "SkyflowCards";
}
