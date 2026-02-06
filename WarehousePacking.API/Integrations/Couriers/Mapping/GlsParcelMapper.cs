using WarehousePacking.API.Services.Shipment.GLS;
using WarehousePacking.Shared.DTOs;

namespace WarehousePacking.API.Integrations.Couriers.Mapping
{
    public class GlsParcelMapper : IParcelMapper<cConsign>
    {
        public cConsign Map(PackageData package)
        {
            return new cConsign
            {
                rname1 = package.Recipient.Name,
                rcountry = package.Recipient.Country,
                rzipcode = package.Recipient.PostalCode,
                rcity = package.Recipient.City,
                rstreet = package.Recipient.Street,
                rphone = package.Recipient.Phone,
                rcontact = package.Recipient.Email,
                notes = package.Description,
                references = package.References,
                quantity = package.PackageQuantity,
                quantitySpecified = true,
                weight = (float)package.Weight,
                weightSpecified = true,
                srv_bool = new cServicesBool
                {
                    pod = package.ShipmentServices.POD,
                    podSpecified = package.ShipmentServices.POD,
                    exw = package.ShipmentServices.EXW,
                    exwSpecified = package.ShipmentServices.EXW,
                    cod = package.ShipmentServices.COD,
                    codSpecified = package.ShipmentServices.COD,
                    cod_amount = (float)package.ShipmentServices.CODAmount,
                    cod_amountSpecified = package.ShipmentServices.COD
                }
            };
        }
    }
}