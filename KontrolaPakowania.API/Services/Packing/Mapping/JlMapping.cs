using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;

namespace KontrolaPakowania.API.Services.Packing.Mapping
{
    public static class JlMapping
    {
        public static IEnumerable<JlData> ToJlData(this IEnumerable<JlDto> jlList)
        {
            foreach (var jl in jlList)
            {
                // If exactly one client, map to flattened JlData
                if (jl.Clients.Count == 1)
                {
                    var client = jl.Clients[0];

                    yield return new JlData
                    {
                        Id = jl.JlId,
                        Barcode = jl.JlCode,
                        Name = jl.JlCode,
                        Status = jl.Status,
                        Weight = jl.Weight,
                        CourierName = client.CourierName,
                        Courier = client.Courier,
                        ShipmentServices = client.ShipmentServices,
                        Country = client.DestinationCountry,
                        ClientId = int.TryParse(client.ClientErpId, out var cid) ? cid : 0,
                        ClientAddressId = client.ClientAddressId,
                        ClientAddressType = client.ClientAddressType,
                        ClientName = client.ClientName,
                        PackageClosed = client.PackageClosed,
                        PackingRequirements = client.PackingRequirements
                    };
                }
                else
                {
                    yield return new JlData
                    {
                        Id = jl.JlId,
                        Barcode = jl.JlCode,
                        Name = jl.JlCode,
                        Status = jl.Status,
                        Weight = jl.Weight,
                        CourierName = "MIX",
                        Courier = Courier.Unknown,
                        ShipmentServices = new(),
                        Country = "MIX",
                        ClientId = 0,
                        ClientAddressId = 0,
                        ClientAddressType = 0,
                        ClientName = "MIX",
                        PackingRequirements = string.Empty,
                    };
                }
            }
        }

        public static JlData ToJlData(this JlDto jlDto)
        {
            if (jlDto == null)
                throw new ArgumentNullException(nameof(jlDto));

            // Map clients: convert string to enum and calculate shipment services
            foreach (var client in jlDto.Clients)
            {
                client.Courier = CourierHelper.GetCourierFromName(client.CourierName);

                var courierLower = client.CourierName.ToLower();
                client.ShipmentServices = new ShipmentServices
                {
                    D12 = courierLower.Contains("12"),
                    D10 = courierLower.Contains("10"),
                    Saturday = courierLower.Contains("sobota"),
                    PZ = courierLower.Contains("zwrotna"),
                    Dropshipping = courierLower.Contains("dropshipping")
                };
            }

            // Single client case
            if (jlDto.Clients.Count == 1)
            {
                var client = jlDto.Clients[0];
                return new JlData
                {
                    Id = jlDto.JlId,
                    Barcode = jlDto.JlCode,
                    Name = jlDto.JlCode,
                    Status = jlDto.Status,
                    Weight = jlDto.Weight,
                    CourierName = client.CourierName,
                    Courier = client.Courier,
                    ShipmentServices = client.ShipmentServices,
                    Country = client.DestinationCountry,
                    ClientId = int.TryParse(client.ClientErpId, out var cid) ? cid : 0,
                    ClientAddressId = client.ClientAddressId,
                    ClientAddressType = client.ClientAddressType,
                    ClientName = client.ClientName,
                    PackageClosed = client.PackageClosed,
                    PackingRequirements = client.PackingRequirements
                };
            }

            // Multiple clients case → assign "MIX"
            return new JlData
            {
                Id = jlDto.JlId,
                Barcode = jlDto.JlCode,
                Name = jlDto.JlCode,
                Status = jlDto.Status,
                Weight = jlDto.Weight,
                CourierName = "MIX",
                Courier = Courier.Unknown,
                ShipmentServices = new ShipmentServices(),
                Country = "MIX",
                OutsideEU = false,
                ClientId = 0,
                ClientAddressId = 0,
                ClientAddressType = 0,
                ClientName = "MIX",
                PackageClosed = false,
                PackingRequirements = string.Empty
            };
        }
    }
}