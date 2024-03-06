using GraphQL;
using GraphQL.Types;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;

namespace VirtoCommerce.Skyflow.XApi.Queries;

public class SkyflowCardQuery : Query<SkyflowCardResponse>
{
    public string? UserId { get; private set; }

    public override IEnumerable<QueryArgument> GetArguments()
    {
        yield return Argument<StringGraphType>(nameof(UserId));
    }

    public override void Map(IResolveFieldContext context)
    {
        UserId = context.GetArgument<string>(nameof(UserId));
    }
}
