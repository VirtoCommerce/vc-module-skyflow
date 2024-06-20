using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Skyflow.XApi.Commands;

public class DeleteSkyflowCardCommand : ICommand<bool>
{
    public required string SkyflowId { get; set; }
    public required string StoreId { get; set; }
}

public class DeleteSkyflowCardCommandType : InputObjectGraphType<DeleteSkyflowCardCommand>
{
    public DeleteSkyflowCardCommandType()
    {
        Field(x => x.SkyflowId, nullable: false).Description("Skyflow card id");
        Field(x => x.StoreId, nullable: false).Description("Store id");
    }
}
