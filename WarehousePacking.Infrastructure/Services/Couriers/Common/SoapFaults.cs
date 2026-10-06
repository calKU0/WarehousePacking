using System.ServiceModel;

namespace WarehousePacking.Infrastructure.Services.Couriers.Common
{
    /// <summary>Turns a SOAP fault into one readable line for the operator.</summary>
    internal static class SoapFaults
    {
        public static string Describe(FaultException fault)
        {
            var parts = new List<string> { fault.Message };

            if (fault.Code is not null)
                parts.Add($"Kod: {fault.Code.Name}");

            var reason = fault.Reason?.GetMatchingTranslation().Text;
            if (!string.IsNullOrEmpty(reason) && reason != fault.Message)
                parts.Add($"Powód: {reason}");

            // Couriers often put the useful part only in the detail element.
            var messageFault = fault.CreateMessageFault();
            if (messageFault.HasDetail)
            {
                using var reader = messageFault.GetReaderAtDetailContents();
                parts.Add($"Szczegóły: {reader.ReadContentAsString()}");
            }

            return string.Join(" | ", parts);
        }
    }
}
