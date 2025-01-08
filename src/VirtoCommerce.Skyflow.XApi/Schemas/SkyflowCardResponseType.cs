using GraphQL.Types;
using VirtoCommerce.Skyflow.XApi.Queries;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Skyflow.XApi.Schemas;

public class SkyflowCardResponseType : ExtendableGraphType<SkyflowCardResponse>
{
    public SkyflowCardResponseType()
    {
        Field<ListGraphType<SkyflowCardType>>("cards").Resolve(context => context.Source?.Cards);
    }
}
