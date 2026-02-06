using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.Enums;
using WarehousePacking.Shared.Helpers;

namespace WarehousePacking.API.Services.Packing.Mapping
{
    public static class JlMapping
    {
        public static IEnumerable<JlData> ToJlData(this IEnumerable<JlDto> jlList)
        {
            foreach (var jl in jlList)
            {
                int status = jl.Status;
                if (jl.ReadyToPack == "TAK" && status != 3)
                    status = 1;
                if (jl.ReadyToPack == "NIE" && status != 3)
                    status = 2;

                var allCourierSymbols = jl.Clients
                    .Select(c => CourierHelper.GetCourierFromName(c.CourierName).GetDescription())
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                // If exactly one client, map to flattened JlData
                if (jl.Clients.Count == 1)
                {
                    var client = jl.Clients[0];

                    yield return new JlData
                    {
                        Id = jl.JlId,
                        Barcode = jl.JlEanCode,
                        Name = jl.JlCode,
                        Destination = jl.DestZone,
                        ReadyToPack = jl.ReadyToPack,
                        Status = status,
                        Weight = jl.Weight,
                        LocationCode = jl.LocationCode,
                        CourierName = client.CourierName,
                        Courier = client.Courier,
                        ShipmentServices = client.ShipmentServices,
                        Country = client.DestinationCountry,
                        ClientId = int.TryParse(client.ClientErpId, out var cid) ? cid : 0,
                        AddressName = client.AddressName,
                        AddressCity = client.AddressCity,
                        AddressStreet = client.AddressStreet,
                        AddressPostalCode = client.AddressPostalCode,
                        AddressCountry = client.AddressCountry,
                        ClientSymbol = client.ClientSymbol,
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
                        Barcode = jl.JlEanCode,
                        Destination = jl.DestZone,
                        LocationCode = jl.LocationCode,
                        ReadyToPack = jl.ReadyToPack,
                        Name = jl.JlCode,
                        Status = status,
                        Weight = jl.Weight,
                        CourierName = "MIX",
                        Courier = Courier.Unknown,
                        ShipmentServices = new(),
                        Country = "MIX",
                        ClientId = 0,
                        AddressName = string.Empty,
                        AddressCity = string.Empty,
                        AddressStreet = string.Empty,
                        AddressPostalCode = string.Empty,
                        AddressCountry = string.Empty,
                        ClientSymbol = "MIX",
                        ClientName = "MIX",
                        PackingRequirements = string.Empty,
                        AllCourierAcronyms = string.Join(", ", allCourierSymbols)
                    };
                }
            }
        }

        public static JlData ToJlData(this JlDto jlDto)
        {
            if (jlDto == null)
                throw new ArgumentNullException(nameof(jlDto));

            int status = jlDto.Status;
            if (jlDto.ReadyToPack == "TAK" && status != 3)
                status = 1;
            if (jlDto.ReadyToPack == "NIE" && status != 3)
                status = 2;

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

            var allCourierSymbols = jlDto.Clients
                .Select(c => CourierHelper.GetCourierFromName(c.CourierName).GetDescription())
                .Distinct(StringComparer.OrdinalIgnoreCase);

            // Single client case
            if (jlDto.Clients.Count == 1)
            {
                var client = jlDto.Clients[0];
                return new JlData
                {
                    Id = jlDto.JlId,
                    LocationCode = jlDto.LocationCode,
                    Destination = jlDto.DestZone,
                    Barcode = jlDto.JlEanCode,
                    ReadyToPack = jlDto.ReadyToPack,
                    Name = jlDto.JlCode,
                    Status = status,
                    Weight = jlDto.Weight,
                    CourierName = client.CourierName,
                    Courier = client.Courier,
                    ShipmentServices = client.ShipmentServices,
                    Country = client.DestinationCountry,
                    ClientId = int.TryParse(client.ClientErpId, out var cid) ? cid : 0,
                    AddressName = client.AddressName,
                    AddressCity = client.AddressCity,
                    AddressStreet = client.AddressStreet,
                    AddressPostalCode = client.AddressPostalCode,
                    AddressCountry = client.AddressCountry,
                    ClientSymbol = client.ClientSymbol,
                    ClientName = client.ClientName,
                    PackageClosed = client.PackageClosed,
                    PackingRequirements = client.PackingRequirements
                };
            }

            // Multiple clients case → assign "MIX"
            return new JlData
            {
                Id = jlDto.JlId,
                Barcode = jlDto.JlEanCode,
                Destination = jlDto.DestZone,
                Name = jlDto.JlCode,
                Status = status,
                ReadyToPack = jlDto.ReadyToPack,
                LocationCode = jlDto.LocationCode,
                Weight = jlDto.Weight,
                CourierName = "MIX",
                Courier = Courier.Unknown,
                ShipmentServices = new ShipmentServices(),
                Country = "MIX",
                OutsideEU = false,
                ClientId = 0,
                AddressName = string.Empty,
                AddressCity = string.Empty,
                AddressStreet = string.Empty,
                AddressPostalCode = string.Empty,
                AddressCountry = string.Empty,
                ClientSymbol = "MIX",
                ClientName = "MIX",
                PackageClosed = false,
                PackingRequirements = string.Empty,
                AllCourierAcronyms = string.Join(", ", allCourierSymbols)
            };
        }
    }
}