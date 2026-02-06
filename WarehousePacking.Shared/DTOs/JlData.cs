using WarehousePacking.Shared.Enums;
using WarehousePacking.Shared.Helpers;

namespace WarehousePacking.Shared.DTOs
{
    public class JlData
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Status { get; set; }
        public decimal Weight { get; set; }
        public string CourierName { get; set; } = string.Empty;
        public string AllCourierAcronyms { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        private Courier courier;

        public Courier Courier
        {
            get => courier;
            set
            {
                courier = value;
                InitCourierLogo();
            }
        }

        public string LogoCourier { get; set; } = string.Empty;
        public ShipmentServices ShipmentServices { get; set; } = new();
        public int Priority { get; set; }
        public int Sorting { get; set; }
        private string country = string.Empty;

        public string Country
        {
            get => country;
            set
            {
                country = value;
                UpdateOutsideEU();
            }
        }

        public string LocationCode { get; set; } = string.Empty;
        public string ReadyToPack { get; set; } = string.Empty;
        public bool OutsideEU { get; set; } = false;
        public int ClientId { get; set; }
        public string ClientSymbol { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string AddressName { get; set; } = string.Empty;
        public string AddressCity { get; set; } = string.Empty;
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressPostalCode { get; set; } = string.Empty;
        public string AddressCountry { get; set; } = string.Empty;
        public bool PackageClosed { get; set; }
        public string InternalBarcode { get; set; } = string.Empty;
        public string PackingRequirements { get; set; } = string.Empty;

        private void InitCourierLogo()
        {
            var suffixes = new List<string>();

            foreach (var prop in typeof(ShipmentServices).GetProperties())
            {
                if (prop.PropertyType == typeof(bool) && (bool)prop.GetValue(ShipmentServices))
                {
                    suffixes.Add(prop.Name); // Or map to user-friendly names
                }
            }

            LogoCourier = suffixes.Any()
                ? $"{Courier.GetDescription()}-{string.Join(", ", suffixes)}"
                : Courier.GetDescription();
        }

        private void UpdateOutsideEU()
        {
            // List of EU countries (simplified example, you can add all EU countries)
            var euCountries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "AT", // Austria
                "BE", // Belgium
                "BG", // Bulgaria
                "HR", // Croatia
                "CY", // Cyprus
                "CZ", // Czech Republic
                "DK", // Denmark
                "EE", // Estonia
                "FI", // Finland
                "FR", // France
                "DE", // Germany
                "GR", // Greece
                "HU", // Hungary
                "IE", // Ireland
                "IT", // Italy
                "LV", // Latvia
                "LT", // Lithuania
                "LU", // Luxembourg
                "MT", // Malta
                "NL", // Netherlands
                "PL", // Poland (Polska)
                "PT", // Portugal
                "RO", // Romania
                "SK", // Slovakia
                "SI", // Slovenia
                "ES", // Spain
                "SE",  // Sweden
                "MIX",
            };

            OutsideEU = !euCountries.Contains(country);
        }
    }
}