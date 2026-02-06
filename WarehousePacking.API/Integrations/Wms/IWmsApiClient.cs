using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;

namespace WarehousePacking.API.Integrations.Wms
{
    public interface IWmsApiClient
    {
        Task<IEnumerable<JlDto>> GetJlListAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jlCode, CancellationToken cancellationToken = default);

        Task<PackWMSResponse> PackStock(PackStockRequest request, CancellationToken cancellationToken = default);

        Task<PackWMSResponse> CloseJl(CloseLuRequest request, CancellationToken cancellationToken = default);
    }
}