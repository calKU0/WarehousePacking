using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class JlData
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Status { get; set; }
        public decimal Weight { get; set; }
        public string CourierName { get; set; } = string.Empty;
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
        public string Country { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
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
    }
}