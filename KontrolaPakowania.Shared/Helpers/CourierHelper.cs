using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.Helpers
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
    }
}