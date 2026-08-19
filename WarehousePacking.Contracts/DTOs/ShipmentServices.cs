using System.Text.RegularExpressions;

namespace WarehousePacking.Contracts.DTOs
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
        private decimal codAmount;

        /// <summary>
        /// Whether there is money to collect on delivery. On the shipping screen
        /// this is the "Pobranie" checkbox, with <see cref="CODAmount"/> as its
        /// amount field — two controls over one fact, so they are kept in step.
        /// </summary>
        public bool COD
        {
            get => cod;
            set => cod = value;
        }

        /// <summary>
        /// The amount to collect. Reads as 0 while <see cref="COD"/> is off, so
        /// nothing downstream — courier mappers, the ERP shipment document — can
        /// charge a pobranie the flag says is not there. Switching the flag off
        /// deliberately does NOT erase what was typed: it used to, and an
        /// operator who toggled the checkbox got a silently blanked amount that
        /// re-ticking never brought back, so the next label went out with no
        /// pobranie at all.
        /// </summary>
        public decimal CODAmount
        {
            get => cod ? codAmount : 0;
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