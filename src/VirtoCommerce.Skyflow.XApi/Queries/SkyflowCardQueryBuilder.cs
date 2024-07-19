using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.Skyflow.XApi.Schemas;
using GraphQL;

namespace VirtoCommerce.Skyflow.XApi.Queries;

public class SkyflowCardQueryBuilder : QueryBuilder<SkyflowCardQuery, SkyflowCardResponse, SkyflowCardResponseType>
{
    protected override string Name => "SkyflowCards";

    public SkyflowCardQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : base(mediator, authorizationService)
    {
    }

    protected override SkyflowCardQuery GetRequest(IResolveFieldContext<object> context)
    {
        var result = base.GetRequest(context);
        return result;
    }
}
