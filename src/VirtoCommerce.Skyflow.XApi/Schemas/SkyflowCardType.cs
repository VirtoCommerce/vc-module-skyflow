using VirtoCommerce.Xapi.Core.Schemas;
using VirtoCommerce.Skyflow.Core.Models;

namespace VirtoCommerce.Skyflow.XApi.Schemas;

public class SkyflowCardType : ExtendableGraphType<SkyflowCard>
{
    public SkyflowCardType()
    {
        Field(x => x.CardExpiration, nullable: true);
        Field(x => x.CardNumber);
        Field(x => x.CardholderName, nullable: true);
        Field(x => x.Cvv, nullable: true);
        Field(x => x.ExpiryMonth, nullable: true);
        Field(x => x.ExpiryYear, nullable: true);
        Field(x => x.SkyflowId);
        Field(x => x.UserId);
    }
}
