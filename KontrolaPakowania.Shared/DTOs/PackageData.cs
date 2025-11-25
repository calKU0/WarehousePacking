using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class PackageData
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        private Courier courier;

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

        public string InternalBarcode { get; set; } = string.Empty;
        public string CourierName { get; set; } = string.Empty;
        public string CourierLogo { get; set; } = string.Empty;
        public PackageType PackageType { get; set; }
        public DocumentStatus Status { get; set; }
        public string InvoiceName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string SenderBankAccount { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string References { get; set; } = string.Empty;
        public string Representative { get; set; } = string.Empty;
        public string RepresentativeEmail { get; set; } = string.Empty;
        public int PackageQuantity { get; set; } = 1;
        public decimal Insurance { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public int WysNumber { get; set; }
        public int WysType { get; set; }
        public bool HasInvoice { get; set; }
        public bool TaxFree { get; set; }
        public bool ManualSend { get; set; }
        public bool ManualEdit { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public Recipient Recipient { get; set; } = new Recipient();
        public ShipmentServices ShipmentServices { get; set; } = new ShipmentServices();

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

            CourierLogo = suffixes.Any()
                ? $"{Courier.GetDescription()}-{string.Join(", ", suffixes)}"
                : Courier.GetDescription();
        }
    }
}