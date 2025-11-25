using KontrolaPakowania.API.Integrations.Couriers.DPD_Romania.DTOs;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using Microsoft.Extensions.Options;

namespace KontrolaPakowania.API.Integrations.Couriers.Mapping
{
    public class DpdRomaniaPackageMapper : IParcelMapper<DpdRomaniaCreateShipmentRequest>
    {
        private readonly DpdRomaniaSettings _settings;

        public DpdRomaniaPackageMapper(IOptions<CourierSettings> options)
        {
            _settings = options?.Value?.DPDRomania ?? throw new ArgumentNullException(nameof(options));
        }

        public DpdRomaniaCreateShipmentRequest Map(PackageData package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            int countryId = package.Recipient.Country switch
            {
                "RO" => 642,
                "BG" => 100,
                "GR" => 300,
                _ => 642
            };

            // Determine service based on weight/country
            int serviceId = GetServiceId(package.Recipient.Country, package.Recipient.City, package.Weight);

            var shipment = new DpdRomaniaCreateShipmentRequest
            {
                UserName = _settings.Username,
                Password = _settings.Password,
                Service = new()
                {
                    ServiceId = serviceId,
                    AutoAdjustPickupDate = true,
                    AdditionalServices = package.ShipmentServices.COD
                        ? new()
                        {
                            COD = new()
                            {
                                Amount = package.ShipmentServices.CODAmount,
                                CurrencyCode = "RON",
                                OBPDetails = new()
                                {
                                    Option = "OPEN",
                                    ReturnShipmentServiceId = serviceId,
                                    ReturnShipmentPayer = "SENDER"
                                },
                                PayoutToThirdParty = false,
                                ProcessingType = "CASH",
                                IncludeShippingPrice = false
                            }
                        }
                        : null
                },
                Content = new()
                {
                    ParcelsCount = 1,
                    TotalWeight = package.Weight,
                    Contents = "AGRICULTURAL PARTS",
                    Package = package.Weight >= 50 ? "PALLET" : "BOX",
                    Parcels = package.Weight >= 50
                        ? new List<DpdRomaniaCreateShipmentRequest.Parcel>
                        {
                            new()
                            {
                                SeqNo = 1,
                                Size = new()
                                {
                                    Depth = package.Length,
                                    Width = package.Width,
                                    Height = package.Height
                                },
                                Weight = package.Weight
                            }
                        }
                        : null
                },
                Payment = new()
                {
                    CourierServicePayer = "SENDER"
                },
                Recipient = new()
                {
                    Phone1 = new() { Number = FormatPhoneNumber(package.Recipient.Phone) },
                    PrivatePerson = true,
                    ClientName = package.Recipient.Name,
                    ContactName = package.Recipient.Name,
                    Email = package.Recipient.Email,
                    Address = new()
                    {
                        CountryId = countryId,
                        PostCode = package.Recipient.PostalCode,
                        SiteName = package.Recipient.City,
                        StreetType = "str.",
                        StreetName = package.Recipient.Street,
                        StreetNo = " "
                    }
                },
                ShipmentNote = package.Description,
                Ref1 = package.References,
                Ref2 = "R2"
            };

            return shipment;
        }

        private int GetServiceId(string country, string city, decimal weight)
        {
            if (weight < 50)
            {
                if (country == "RO")
                {
                    return city.Equals("oradea", StringComparison.OrdinalIgnoreCase)
                        ? 2114 // LOCO service
                        : 2003;
                }
                else if (country == "BG" || country == "HU" || country == "GR")
                {
                    return 2212;
                }
            }
            else
            {
                if (country == "RO") return 2412;
                if (country == "BG") return 2432;
            }

            return 2003;
        }

        private static string FormatPhoneNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Trim spaces
            var phone = input.Trim();

            // Keep only digits, '+', and spaces
            phone = new string(phone.Where(c => char.IsDigit(c) || c == '+' || c == ' ').ToArray());

            // If starts with a digit but not '0', add '+'
            if (phone.Length > 0 && char.IsDigit(phone[0]) && phone[0] != '0')
            {
                phone = "+" + phone;
            }

            // Ensure it starts only with '0' or '+'
            if (!(phone.StartsWith("0") || phone.StartsWith("+")))
            {
                phone = "+" + phone;
            }

            // Limit to 20 characters
            if (phone.Length > 20)
            {
                phone = phone.Substring(0, 20);
            }

            return phone;
        }
    }
}