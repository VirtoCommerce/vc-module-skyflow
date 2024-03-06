using GraphQL.Types;
using VirtoCommerce.ExperienceApiModule.Core.Schemas;
using VirtoCommerce.Skyflow.XApi.Queries;

namespace VirtoCommerce.Skyflow.XApi.Schemas;

public class SkyflowCardResponseType : ExtendableGraphType<SkyflowCardResponse>
{
    public SkyflowCardResponseType()
    {
        Field<ListGraphType<SkyflowCardType>>("cards", resolve: context => context.Source?.Cards);
    }
}
