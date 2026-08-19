using System.Text.Json.Serialization;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class PackWMSResponse
    {
        /// <summary>
        /// Text the WMS returns when the JL is not in the state the operation
        /// expects, e.g. "Nieprawidłowy status JL: 0. Oczekiwano: Gotowa do pakowania (12)".
        /// </summary>
        private const string AlreadyAppliedMarker = "Nieprawidłowy status JL";

        public string Status { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;

        [JsonIgnore]
        public bool IsSuccess => Status == "1";

        /// <summary>
        /// True when the WMS refused the call because the JL had already moved
        /// past the state this operation starts from. That is not a failure: the
        /// stock is packed / the JL is closed, we just never got to hear it the
        /// first time — typically a request that timed out on the way back.
        /// Treating it as done is what makes a timed-out call safe to retry.
        /// </summary>
        [JsonIgnore]
        public bool IsAlreadyApplied =>
            !IsSuccess
            && !string.IsNullOrEmpty(Desc)
            && Desc.Contains(AlreadyAppliedMarker, System.StringComparison.OrdinalIgnoreCase);
    }
}