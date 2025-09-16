using KontrolaPakowania.API.Services.Couriers.GLS;
using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Services.Couriers.Mapping
{
    public class GlsParcelMapper : IParcelMapper<cConsign>
    {
        public cConsign Map(PackageInfo package)
        {
            return new cConsign
            {
                rname1 = package.RecipientName,
                rcountry = package.RecipientCountry,
                rzipcode = package.RecipientPostalCode,
                rcity = package.RecipientCity,
                rstreet = package.RecipientStreet,
                rphone = package.RecipientPhone,
                rcontact = package.RecipientEmail,
                notes = package.Description,
                references = package.References,
                quantity = package.PackageQuantity,
                quantitySpecified = true,
                weight = (float)package.Weight,
                weightSpecified = true,
                srv_bool = new cServicesBool
                {
                    pod = package.Services.POD,
                    podSpecified = package.Services.POD,
                    exw = package.Services.EXW,
                    exwSpecified = package.Services.EXW,
                    cod = package.Services.COD,
                    codSpecified = package.Services.COD,
                    cod_amount = (float)package.Services.CODAmount,
                    cod_amountSpecified = package.Services.COD
                }
            };
        }
    }
}