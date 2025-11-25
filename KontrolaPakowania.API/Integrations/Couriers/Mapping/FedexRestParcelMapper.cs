using FedexServiceReference;
using KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using Microsoft.Extensions.Options;

namespace KontrolaPakowania.API.Integrations.Couriers.Mapping
{
    public class FedexRestParcelMapper : IParcelMapper<FedexShipmentRequest>
    {
        private readonly FedexRestSettings _settings;
        private readonly SenderSettings _senderSettings;

        public FedexRestParcelMapper(IOptions<CourierSettings> options)
        {
            _settings = options?.Value?.Fedex.Rest ?? throw new ArgumentNullException(nameof(options));
            _senderSettings = options?.Value?.Sender ?? throw new ArgumentNullException(nameof(options));
        }

        public FedexShipmentRequest Map(PackageData package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var shipment = new FedexShipmentRequest
            {
                LabelResponseOptions = "LABEL",
                AccountNumber = new AccountNumber { Value = _settings.Account },
                RequestedShipment = MapRequestedShipment(package)
            };

            return shipment;
        }

        private RequestedShipment MapRequestedShipment(PackageData package)
        {
            RequestedShipment shipment = new()
            {
                ShipDatestamp = DateTime.Now.Date.ToString("yyyy-MM-dd"),
                PickupType = "CONTACT_FEDEX_TO_SCHEDULE",
                ServiceType = "INTERNATIONAL_ECONOMY",
                PackagingType = "YOUR_PACKAGING",
                TotalWeight = (double)package.Weight,
                ShippingChargesPayment = new ShippingChargesPayment { PaymentType = "SENDER" },
                LabelSpecification = new LabelSpecification { ImageType = "ZPLII", LabelStockType = "STOCK_4X6" },
                RequestedPackageLineItems = new List<RequestedPackageLineItem>
                {
                    new RequestedPackageLineItem
                    {
                        Weight = new Weight
                        {
                            Units = "KG",
                            Value = (int)package.Weight,
                        }
                    }
                },
                PreferredCurrency = "PLN",
                TotalPackageCount = 1,
                CustomsClearanceDetail = MapCustomsClearanceDetail(package),
                Shipper = MapShipper(),
                Recipients = MapRecipents(package),
            };
            return shipment;
        }

        private Shipper MapShipper()
        {
            Shipper shipper = new()
            {
                Address = new Address
                {
                    City = _senderSettings.City,
                    PostalCode = _senderSettings.PostalCode,
                    CountryCode = _senderSettings.Country,
                    StreetLines = new List<string> { _senderSettings.Street }
                },
                Contact = new Contact
                {
                    PersonName = _senderSettings.PersonName,
                    EmailAddress = _senderSettings.Email,
                    PhoneExtension = "48",
                    PhoneNumber = _senderSettings.Phone.Replace("+48", ""),
                    CompanyName = _senderSettings.Company,
                }
            };

            return shipper;
        }

        private List<Fedex.DTOs.Recipient> MapRecipents(PackageData package)
        {
            List<Fedex.DTOs.Recipient> recipents = new()
            {
                new Fedex.DTOs.Recipient
                {
                    Address = new Address
                    {
                        City = package.Recipient.City,
                        PostalCode = package.Recipient.PostalCode,
                        CountryCode = package.Recipient.Country,
                        StreetLines = new List<string> { package.Recipient.Street }
                    },
                    Contact = new Contact
                    {
                        EmailAddress = package.Recipient.Email,
                        PhoneNumber = package.Recipient.Phone,
                        CompanyName = package.Recipient.Name,
                    }
                }
            };
            return recipents;
        }

        private CustomsClearanceDetail MapCustomsClearanceDetail(PackageData package)
        {
            CustomsClearanceDetail customs = new()
            {
                DutiesPayment = new()
                {
                    PaymentType = "SENDER",
                    Payor = new()
                    {
                        ResponsibleParty = new()
                        {
                            AccountNumber = new()
                            {
                                Value = _settings.Account
                            }
                        }
                    }
                },
                IsDocumentOnly = false,
                TotalCustomsValue = new TotalCustomsValue
                {
                    Amount = (double)package.Insurance,
                    Currency = "PLN"
                },
                Commodities = new()
                {
                    new Commodity
                    {
                        Description = "SPARE PARTS FOR AGRICULTURAL MACHINES",
                        CountryOfManufacture = "PL",
                        HarmonizedCode = "1234567890",
                        Weight = new()
                        {
                            Units = "KG",
                            Value = (int)package.Weight
                        },
                        Quantity = 1,
                        QuantityUnits = "PCS",
                        UnitPrice = new()
                        {
                            Amount = 0,
                            Currency = "PLN"
                        },
                        CustomsValue = new()
                        {
                            Amount = "",
                            Currency = "PLN"
                        },
                        ExportLicenseExpirationDate = DateTime.Now.AddDays(999)
                    }
                }
            };

            return customs;
        }
    }
}