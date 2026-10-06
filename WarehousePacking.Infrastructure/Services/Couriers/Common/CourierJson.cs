using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarehousePacking.Infrastructure.Services.Couriers.Common
{
    /// <summary>JSON conventions shared by the REST courier APIs.</summary>
    internal static class CourierJson
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions ReadOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Serialized up front rather than streamed, so the request carries a
        /// Content-Length — some courier gateways reject chunked bodies.
        /// </summary>
        public static StringContent ToContent(object payload) =>
            new(JsonSerializer.Serialize(payload, WriteOptions), Encoding.UTF8, "application/json");

        /// <summary>Returns null when the body is empty or is not the expected JSON.</summary>
        public static T? TryDeserialize<T>(string body) where T : class
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(body, ReadOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
