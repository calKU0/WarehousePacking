using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class JlDto
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Status { get; set; }
        public decimal Weight { get; set; }
        public string CourierName { get; set; } = string.Empty;
        public Courier Courier { get; set; } = Courier.Unknown;
        public string LogoCourier { get; set; } = string.Empty;
        public CourierServices CourierServices { get; set; } = new();
        public int RouteId { get; set; }
        public int Priority { get; set; }
        public int Sorting { get; set; }
        public bool OutsideEU { get; set; } = false;
        public int ClientId { get; set; }
        public int ClientAddressId { get; set; }
        public string ClientName { get; set; } = string.Empty;

        public void InitCourierFromName()
        {
            // Map string to enum
            var courierLower = CourierName.ToLower();
            Courier = courierLower switch
            {
                var c when c.Contains("fedex") => Courier.Fedex,
                var c when c.Contains("dpd-romania") => Courier.DPD_Romania,
                var c when c.Contains("dpd") => Courier.DPD,
                var c when c.Contains("gls") => Courier.GLS,
                var c when c.Contains("odbiór własny") => Courier.Personal_Collection,
                var c when c.Contains("hellmann") => Courier.Hellmann,
                var c when c.Contains("shenker") => Courier.Shenker,
                _ => Courier.Unknown
            };
        }

        public void InitCourierLogo()
        {
            // Build LogoCourier string based on CourierServices
            var suffixes = new List<string>();
            if (CourierServices.Saturday) suffixes.Add("Sobota");
            if (CourierServices.Return) suffixes.Add("zwrotna");
            if (CourierServices._12) suffixes.Add("1200");
            if (CourierServices.Dropshipping) suffixes.Add("Dropshipping");

            LogoCourier = suffixes.Any()
                ? $"{Courier}-{string.Join(", ", suffixes)}"
                : Courier.ToString();
        }
    }

    public class CourierServices
    {
        public bool Return { get; set; }
        public bool _12 { get; set; }
        public bool Saturday { get; set; }
        public bool Dropshipping { get; set; }
    }
}