using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using System.Data;
using System.Reflection.Metadata;

namespace KontrolaPakowania.API.Services.Shipment
{
    public class ShipmentService : IShipmentService
    {
        private readonly IDbExecutor _db;

        public ShipmentService(IDbExecutor db)
        {
            _db = db;
        }

        public async Task<int> CreateErpShipmentDocument(ShipmentResponse shipment)
        {
            const string procedure = "kp.CreateShipmentDocument";
            return await _db.QuerySingleOrDefaultAsync<int>(procedure, new { shipment.PackageId, shipment.TrackingNumber, shipment.TrackingLink, shipment.PackageInfo.Services.CODAmount, shipment.PackageInfo.Insurance }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> AddErpAttributes(int documentId, PackageInfo packageInfo)
        {
            const string procedure = "kp.AddShipmentAttributes";

            string ROD = packageInfo.Services.ROD ? "TAK" : "NIE";
            string POD = packageInfo.Services.POD ? "TAK" : "NIE";
            string EXW = packageInfo.Services.EXW ? "TAK" : "NIE";
            string S10 = packageInfo.Services.S10 ? "TAK" : "NIE";
            string S12 = packageInfo.Services.S12 ? "TAK" : "NIE";
            string Saturday = packageInfo.Services.Saturday ? "TAK" : "NIE";
            string COD = packageInfo.Services.COD ? "TAK" : "NIE";

            var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { POD, ROD, EXW, S10, S12, Saturday, COD, documentId }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result == 7;
        }
    }
}