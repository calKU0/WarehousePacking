using Microsoft.Extensions.Logging;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Contracts.Services;
using WarehousePacking.Infrastructure.Helpers;

namespace WarehousePacking.Infrastructure.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly ILogger<ShipmentService> _logger;

        public ShipmentService(IShipmentRepository shipmentRepository, ILogger<ShipmentService> logger)
        {
            _shipmentRepository = shipmentRepository;
            _logger = logger;
        }

        public async Task<int> CreateErpShipmentDocument(ShipmentResponse shipment)
        {
            string courier = shipment.PackageInfo.Courier.GetDescription();
            _logger.LogInformation("Creating ERP shipment document for package {PackageId}, courier {Courier}", shipment.PackageId, courier);
            var result = await _shipmentRepository.CreateErpShipmentDocument(shipment, courier);
            _logger.LogInformation("Created ERP shipment document result for package {PackageId}: {Result}", shipment.PackageId, result);
            return result;
        }

        public async Task<bool> DeleteErpShipmentDocument(int wysNumber, int wysType)
        {
            var result = await _shipmentRepository.DeleteErpShipmentDocument(wysNumber, wysType);
            _logger.LogInformation("Delete ERP shipment document for WYS {WysNumber}/{WysType}: {Succeeded}", wysNumber, wysType, result);
            return result;
        }

        public async Task<bool> AddErpAttributes(int documentId, ShipmentResponse shipmentInfo)
        {
            var result = await _shipmentRepository.AddErpAttributes(documentId, shipmentInfo);
            _logger.LogInformation("Add ERP attributes for document {DocumentId}: {Succeeded}", documentId, result);
            return result;
        }

        public async Task<PackageData?> GetShipmentDataByBarcode(string barcode)
        {
            var result = await _shipmentRepository.GetShipmentDataByBarcode(barcode);

            if (result is not null)
            {
                result.Courier = CourierHelper.GetCourierFromName(result.CourierName);
            }

            _logger.LogInformation("Fetched shipment data for barcode {Barcode}: {Found}", barcode, result is not null);

            return result;
        }

        public async Task<IEnumerable<Recipient>?> SearchAddress(string code)
        {
            return await _shipmentRepository.SearchAddress(code);
        }

        public async Task<IEnumerable<SearchInvoiceResult>?> SearchInvoice(string code)
        {
            return await _shipmentRepository.SearchInvoice(code);
        }

        public async Task<RoutesStatus> GetRoutesStatus()
        {
            var status = await _shipmentRepository.GetRoutesStatus();
            _logger.LogInformation("Fetched routes status");
            return status;
        }

        public async Task<IEnumerable<RoutePackages>> GetRoutePackages(Courier courier)
        {
            string courierName = courier.GetDescription();
            var packages = await _shipmentRepository.GetRoutePackages(courierName);
            _logger.LogInformation("Fetched route packages for courier {Courier}", courierName);
            return packages;
        }

        public async Task<int> CloseRoute(Courier courier)
        {
            string courierName = courier.GetDescription();
            var result = await _shipmentRepository.CloseRoute(courierName);
            _logger.LogInformation("Close route for courier {Courier} result: {Result}", courierName, result);
            return result;
        }
    }
}