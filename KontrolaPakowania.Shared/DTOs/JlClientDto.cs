using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class JlClientDto
    {
        [JsonPropertyName("courier")]
        public string CourierName { get; set; } = string.Empty;

        private Courier courier;

        [JsonPropertyName("courierenum")]
        public Courier Courier
        {
            get => courier;
            set
            {
                if (courier != value)
                {
                    courier = value;
                    InitCourierLogo();
                }
            }
        }

        public string LogoCourier { get; set; } = string.Empty;

        public string ClientErpId { get; set; } = string.Empty;

        public int ClientAddressId { get; set; }

        public int ClientAddressType { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public string DestinationCountry { get; set; } = string.Empty;

        public ShipmentServices ShipmentServices { get; set; } = new();

        public bool PackageClosed { get; set; }

        public string PackingRequirements { get; set; } = string.Empty;

        private void InitCourierLogo()
        {
            var suffixes = new List<string>();

            foreach (var prop in typeof(ShipmentServices).GetProperties())
            {
                if (prop.PropertyType == typeof(bool) && (bool)prop.GetValue(ShipmentServices))
                {
                    suffixes.Add(prop.Name);
                }
            }

            LogoCourier = suffixes.Any()
                ? $"{Courier.GetDescription()}-{string.Join(", ", suffixes)}"
                : Courier.GetDescription();
        }
    }
}