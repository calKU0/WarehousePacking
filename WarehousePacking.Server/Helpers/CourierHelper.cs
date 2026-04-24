using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;

namespace WarehousePacking.Server.Helpers
{
    public static class CourierHelper
    {
        public static readonly Courier[] AllowedCouriersForLabel =
        {
            Courier.GLS,
            Courier.DPD,
            Courier.DPD_Romania,
            Courier.Fedex
        };

        private static readonly Dictionary<string, Courier> CourierMapping = new()
        {
            ["fedex"] = Courier.Fedex,
            ["dpd-romania"] = Courier.DPD_Romania,
            ["dpd"] = Courier.DPD,
            ["gls"] = Courier.GLS,
            ["odbiór własny"] = Courier.Personal_Collection,
            ["hellmann"] = Courier.Hellmann,
            ["transport na zlecenie"] = Courier.Transport_On_Request,
            ["trans. na zlecenie"] = Courier.Transport_On_Request,
            ["transport odbiorcy"] = Courier.Recipient_Transport,
            ["transport dostawcy"] = Courier.Supplier_Transport,
            ["raben"] = Courier.Raben,
            ["schenker"] = Courier.Schenker,
            ["suus"] = Courier.Suus,
            ["dachser"] = Courier.Dachser,
            ["diera"] = Courier.Diera
        };

        public static Courier GetCourierFromName(string name)
        {
            var lower = name.ToLower();
            foreach (var kvp in CourierMapping)
            {
                if (lower.Contains(kvp.Key))
                    return kvp.Value;
            }
            return Courier.Unknown;
        }

        public static string GetCourierLogo(ShipmentServices? shipmentServices, Courier? courier)
        {
            if (courier is null || courier == Courier.Unknown)
                return string.Empty;

            string courierName = courier.GetDescription().Replace(":", "").Trim();
            var suffixes = new List<string>();

            if (shipmentServices != null)
            {
                foreach (var prop in typeof(ShipmentServices).GetProperties())
                {
                    if (prop.PropertyType == typeof(bool) && (bool)prop.GetValue(shipmentServices)!)
                    {
                        suffixes.Add(prop.Name);
                    }
                }
            }

            string basePath = "images/couriers/";
            string wwwRoot = Path.Combine(Environment.CurrentDirectory, "wwwroot");

            if (suffixes.Any())
            {
                string logoWithSuffix = $"{courierName}-{string.Join(", ", suffixes)}";
                string pathWithSuffix = TryGetImagePath(basePath, logoWithSuffix, wwwRoot);

                if (!string.IsNullOrEmpty(pathWithSuffix))
                    return pathWithSuffix;
            }

            return TryGetImagePath(basePath, courierName, wwwRoot);
        }

        private static string TryGetImagePath(string basePath, string fileName, string wwwRoot)
        {
            string[] extensions = { ".png", ".jpg" };

            foreach (var ext in extensions)
            {
                string relativePath = $"{basePath}{fileName}{ext}";

                string physicalPath = Path.Combine(wwwRoot, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (File.Exists(physicalPath))
                    return relativePath;
            }

            return string.Empty;
        }
    }
}