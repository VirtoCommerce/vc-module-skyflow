using GraphQL;
using GraphQL.Types;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;

namespace VirtoCommerce.Skyflow.XApi.Queries;

public class SkyflowCardQuery : Query<SkyflowCardResponse>
{
    public string? UserId { get; private set; }
    public string StoreId { get; private set; } = string.Empty;

    public override IEnumerable<QueryArgument> GetArguments()
    {
        yield return Argument<StringGraphType>(nameof(UserId));
        yield return Argument<StringGraphType>(nameof(StoreId));
    }

    public override void Map(IResolveFieldContext context)
    {
        UserId = context.GetArgument<string>(nameof(UserId));
        StoreId = context.GetArgument<string>(nameof(StoreId))!;
    }
}
