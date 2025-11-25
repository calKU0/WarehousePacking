using FedexServiceReference;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using Microsoft.Extensions.Options;

namespace KontrolaPakowania.API.Integrations.Couriers.Mapping
{
    public class FedexSoapParcelMapper : IParcelMapper<listV2>
    {
        private readonly FedexSoapSettings _settings;
        private readonly SenderSettings _senderSettings;

        public FedexSoapParcelMapper(IOptions<CourierSettings> options)
        {
            _settings = options?.Value?.Fedex.Soap ?? throw new ArgumentNullException(nameof(options));
            _senderSettings = options?.Value?.Sender ?? throw new ArgumentNullException(nameof(options));
        }

        public listV2 Map(PackageData package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var list = new listV2
            {
                sender = MapSender(package),
                receiver = MapReceiver(package),
                parcels = MapParcels(package),
                proofOfDispatch = MapProofOfDispatch(),
                remarks = package.Description,
                shipmentType = "K",
                paymentForm = "P",
                payerType = "1"
            };

            MapCodAndInsurance(package, list);
            MapAdditionalServices(package, list);

            return list;
        }

        private nadawcaV2 MapSender(PackageData package)
        {
            var senderId = package.ShipmentServices.Dropshipping
                ? _settings.DropshippingSenderId
                : _settings.SenderId;

            return new nadawcaV2
            {
                senderId = senderId,
                contactDetails = new daneKontaktowe
                {
                    email = _senderSettings.Email,
                    phoneNo = _senderSettings.Phone,
                    surname = _senderSettings.PersonName,
                    name = _senderSettings.Company
                }
            };
        }

        private odbiorcaV2 MapReceiver(PackageData package)
        {
            var receiver = new odbiorcaV2
            {
                addressDetails = new daneAdresowe
                {
                    city = package.Recipient.City,
                    postalCode = package.Recipient.PostalCode,
                    countryCode = package.Recipient.Country,
                    street = package.Recipient.Street,
                    isCompany = package.Recipient.Type.ToString(),
                    homeNo = string.Empty
                },
                contactDetails = new daneKontaktowe
                {
                    phoneNo = package.Recipient.Phone,
                    email = package.Recipient.Email
                }
            };

            if (package.Recipient.Type == 1)
            {
                receiver.addressDetails.companyName = package.Recipient.Name;
            }
            else
            {
                receiver.addressDetails.surname = package.Recipient.Name;
                receiver.contactDetails.surname = package.Recipient.Name;
            }

            return receiver;
        }

        private paczkaV2[] MapParcels(PackageData package)
        {
            var parcels = new paczkaV2[package.PackageQuantity];
            for (int i = 0; i < parcels.Length; i++)
            {
                parcels[i] = new paczkaV2
                {
                    type = package.PackageType.ToString(),
                    weight = FedexHelper.ToNumberString(package.Weight),
                    shape = "0",
                    nrExtPp = package.References
                };
            }
            return parcels;
        }

        private potwierdzenieNadaniaV2 MapProofOfDispatch()
        {
            return new potwierdzenieNadaniaV2
            {
                sendDate = FedexHelper.ToDateString(DateTime.Now),
                senderSignature = "System",
                courierId = _settings.CourierId.ToString()
            };
        }

        private void MapCodAndInsurance(PackageData package, listV2 list)
        {
            //if (package.ShipmentServices.COD && package.ShipmentServices.CODAmount > 0)
            //{
            list.cod = new pobranieV2
            {
                codValue = FedexHelper.ToNumberString(100),
                bankAccountNumber = package.SenderBankAccount,
                codType = "B"
            };

            if (package.Insurance == 0)
            {
                package.Insurance = Math.Max(package.ShipmentServices.CODAmount, 5000);
            }
            //}

            if (package.Insurance > 0)
            {
                list.insurance = new ubezpieczenieV2
                {
                    insuranceValue = FedexHelper.ToNumberString(package.Insurance),
                    contentDescription = package.References
                };
            }
        }

        private void MapAdditionalServices(PackageData package, listV2 list)
        {
            var services = new List<uslugaDodatkowa>();

            if (package.ShipmentServices.D10) services.Add(new uslugaDodatkowa { serviceId = "ND10" });
            if (package.ShipmentServices.D12) services.Add(new uslugaDodatkowa { serviceId = "ND12" });
            if (package.ShipmentServices.ROD) services.Add(new uslugaDodatkowa { serviceId = "DOC_RETURN" });
            if (package.ShipmentServices.Saturday) services.Add(new uslugaDodatkowa { serviceId = "SATURDAY_DELIVERY" });
            if (package.ShipmentServices.PZ) services.Add(new uslugaDodatkowa { serviceId = "PAZ" });

            if (services.Count > 0)
            {
                // Convert each service to jagged array as required by FedEx SOAP
                foreach (var service in services)
                {
                    service.serviceArguments = new argumentUslugi[0]; // correct: argumentUslugi[][]
                }

                list.additionalServices = services.ToArray();
            }
        }
    }
}

internal static class FedexHelper
{
    public static string ToNumberString(object value)
    {
        return value?.ToString().Replace(".", ",");
    }

    public static string ToDateString(DateTime date)
    {
        return date.ToString("yyyy-MM-dd HH:mm");
    }
}