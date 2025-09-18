using KontrolaPakowania.API.Services.Shipment.DPD.Reference;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.Shipment.Mapping
{
    public class DpdPackageMapper : IParcelMapper<DpdCreatePackageRequest>
    {
        public DpdCreatePackageRequest Map(PackageInfo packageInfo)
        {
            List<DpdCreatePackageRequest.Package> packages = new();
            List<DpdCreatePackageRequest.Parcel> parcels = new();
            List<DpdCreatePackageRequest.Service> services = new();

            DpdCreatePackageRequest.Parcel parcel = new()
            {
                Weight = Math.Min(Math.Round(packageInfo.Weight, 2), 9999.99m),
                SizeX = packageInfo.Length < 1 ? 1 : packageInfo.Length,
                SizeY = packageInfo.Width < 1 ? 1 : packageInfo.Width,
                SizeZ = packageInfo.Height < 1 ? 1 : packageInfo.Height,
                Content = "AGRICULTURAL PARTS"
            };
            parcels.Add(parcel);

            // COD (Cash on Delivery)
            if (packageInfo.Services.COD)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "COD",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>
                    {
                        new() { Code = "AMOUNT", Value = packageInfo.Services.CODAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
                        new() { Code = "CURRENCY", Value = "PLN" }
                    }
                });
            }

            // ROD (Return of Document)
            if (packageInfo.Services.ROD)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "ROD",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>()
                });
            }

            // POD (Proof of Delivery / Hand delivery)
            if (packageInfo.Services.POD)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "HAND_DELIVERY",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>()
                });
            }

            // S10 -> Time fixed (10:00)
            if (packageInfo.Services.S10)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "TIME_FIXED",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>
                {
                    new() { Code = "VALUE", Value = "10:00" }
                }
                });
            }

            // S12 -> Delivery before 12:00
            if (packageInfo.Services.S12)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "TIME1200",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>()
                });
            }

            // Saturday delivery
            if (packageInfo.Services.Saturday)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "SATURDAY",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>()
                });
            }

            // EXW (ex works – return shipment / CUD)
            if (packageInfo.Services.EXW)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "CUD",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>()
                });
            }

            DpdCreatePackageRequest.Package package = new()
            {
                Receiver = new()
                {
                    Company = packageInfo.RecipientName,
                    Name = packageInfo.RecipientName,
                    Address = packageInfo.RecipientStreet,
                    City = packageInfo.RecipientCity,
                    CountryCode = packageInfo.RecipientCountry,
                    PostalCode = packageInfo.RecipientPostalCode.Replace("-", ""),
                    Phone = string.IsNullOrEmpty(packageInfo.RecipientPhone) ? null : packageInfo.RecipientPhone,
                    Email = string.IsNullOrEmpty(packageInfo.RecipientEmail) ? null : packageInfo.RecipientEmail
                },
                Sender = new()
                {
                    Company = "GĄSKA sp. z o.o.",
                    Name = "GĄSKA sp. z o.o.",
                    Address = "Gotkowice 85",
                    City = "Jerzmanowice",
                    CountryCode = "PL",
                    PostalCode = "32048",
                    Phone = "123890941",
                    Email = "kontakt@gaska.com.pl"
                },
                PayerFID = 1495,
                Ref1 = packageInfo.References ?? null,
                Ref2 = packageInfo.Description ?? null,
                Parcels = parcels,
                Services = services
            };

            packages.Add(package);

            return new DpdCreatePackageRequest
            {
                GenerationPolicy = "ALL_OR_NOTHING",
                Packages = packages
            };
        }
    }
}