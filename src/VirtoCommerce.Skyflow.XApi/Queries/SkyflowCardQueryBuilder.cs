using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.Skyflow.XApi.Schemas;

namespace VirtoCommerce.Skyflow.XApi.Queries;

public class SkyflowCardQueryBuilder : QueryBuilder<SkyflowCardQuery, SkyflowCardResponse, SkyflowCardResponseType>
{
    protected override string Name => "SkyflowCards";

    public SkyflowCardQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : base(mediator, authorizationService)
    {
    }    
}
