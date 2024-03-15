using VirtoCommerce.Skyflow.Core.Models;

namespace VirtoCommerce.Skyflow.XApi.Queries;

public class SkyflowCardResponse
{
    public IEnumerable<SkyflowCard> Cards { get; set; } = Array.Empty<SkyflowCard>();
}
