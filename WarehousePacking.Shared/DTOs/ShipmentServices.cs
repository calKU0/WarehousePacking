using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs
{
    public class ShipmentServices
    {
        public bool POD { get; set; }
        public bool EXW { get; set; }
        public bool ROD { get; set; }
        public bool D10 { get; set; }
        public bool D12 { get; set; }
        public bool PZ { get; set; }
        public bool Dropshipping { get; set; }
        public bool Saturday { get; set; }
        private bool cod;

        public bool COD
        {
            get => cod;
            set
            {
                cod = value;
                if (!cod) CODAmount = 0;
            }
        }

        private decimal codAmount;

        public decimal CODAmount
        {
            get => codAmount;
            set
            {
                codAmount = value;
                cod = codAmount > 0;
            }
        }

        public static ShipmentServices FromString(string input)
        {
            var services = new ShipmentServices();
            var lowerInput = input.ToLower();

            foreach (var kvp in ServiceMapping)
            {
                if (!lowerInput.Contains(kvp.Key))
                    continue;

                string? value = null;

                // Special handling for codamount
                if (kvp.Key == "codamount")
                {
                    // codamount=123.45
                    var match = Regex.Match(
                        lowerInput,
                        @"codamount\s*=\s*([\d.,]+)");

                    if (match.Success)
                        value = match.Groups[1].Value;
                }

                kvp.Value(services, value);
            }

            return services;
        }

        public bool HasAnyService()
        {
            return typeof(ShipmentServices)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(bool))
                .Any(p => (bool)p.GetValue(this));
        }

        private static readonly Dictionary<string, Action<ShipmentServices, string>> ServiceMapping =
            new()
            {
                ["10"] = (s, _) => s.D10 = true,
                ["12"] = (s, _) => s.D12 = true,
                ["sobota"] = (s, _) => s.Saturday = true,
                ["zwrotna"] = (s, _) => s.PZ = true,
                ["dropshipping"] = (s, _) => s.Dropshipping = true,
                ["cod"] = (s, _) => s.COD = true,
                ["exw"] = (s, _) => s.EXW = true,
                ["codamount"] = (s, value) =>
                {
                    if (decimal.TryParse(value, out var amount))
                        s.CODAmount = amount;
                }
            };
    }
}