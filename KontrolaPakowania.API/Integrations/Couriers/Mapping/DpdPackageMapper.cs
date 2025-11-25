using KontrolaPakowania.API.Integrations.Couriers.DPD.DTOs;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using Microsoft.Extensions.Options;

namespace KontrolaPakowania.API.Integrations.Couriers.Mapping
{
    public class DpdPackageMapper : IParcelMapper<DpdCreatePackageRequest>
    {
        private readonly DpdSettings _settings;
        private readonly SenderSettings _senderSettings;

        public DpdPackageMapper(IOptions<CourierSettings> options)
        {
            _settings = options?.Value?.DPD ?? throw new ArgumentNullException(nameof(options));
            _senderSettings = options?.Value?.Sender ?? throw new ArgumentNullException(nameof(options));
        }

        public DpdCreatePackageRequest Map(PackageData packageInfo)
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
            if (packageInfo.ShipmentServices.COD)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "COD",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>
                    {
                        new() { Code = "AMOUNT", Value = packageInfo.ShipmentServices.CODAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
                        new() { Code = "CURRENCY", Value = "PLN" }
                    }
                });
            }

            // ROD (Return of Document)
            if (packageInfo.ShipmentServices.ROD)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "ROD",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>()
                });
            }

            // POD (Proof of Delivery / Hand delivery)
            if (packageInfo.ShipmentServices.POD)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "HAND_DELIVERY",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>()
                });
            }

            // S10 -> Time fixed (10:00)
            if (packageInfo.ShipmentServices.D10)
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
            if (packageInfo.ShipmentServices.D12)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "TIME1200",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>()
                });
            }

            // Saturday delivery
            if (packageInfo.ShipmentServices.Saturday)
            {
                services.Add(new DpdCreatePackageRequest.Service
                {
                    Code = "SATURDAY",
                    Attributes = new List<DpdCreatePackageRequest.Attribute>()
                });
            }

            // PZ (Return shipment / CUD)
            if (packageInfo.ShipmentServices.PZ)
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
                    Company = packageInfo.Recipient.Name,
                    Name = packageInfo.Recipient.Name,
                    Address = packageInfo.Recipient.Street,
                    City = packageInfo.Recipient.City,
                    CountryCode = packageInfo.Recipient.Country,
                    PostalCode = packageInfo.Recipient.PostalCode.Replace("-", ""),
                    Phone = string.IsNullOrEmpty(packageInfo.Recipient.Phone) ? null : packageInfo.Recipient.Phone,
                    Email = string.IsNullOrEmpty(packageInfo.Recipient.Email) ? null : packageInfo.Recipient.Email
                },
                Sender = new()
                {
                    Company = _senderSettings.Company,
                    Name = _senderSettings.PersonName,
                    Address = _senderSettings.Street,
                    City = _senderSettings.City,
                    CountryCode = _senderSettings.Country,
                    PostalCode = _senderSettings.PostalCode.Replace("-", ""),
                    Phone = _senderSettings.Phone,
                    Email = _senderSettings.Email
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