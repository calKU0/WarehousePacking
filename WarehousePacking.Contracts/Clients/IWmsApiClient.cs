using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Clients
{
    public interface IWmsApiClient
    {
        Task<IEnumerable<JlDto>> GetJlListAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jlCode, CancellationToken cancellationToken = default);

        Task<PackWMSResponse> PackStock(PackStockRequest request, CancellationToken cancellationToken = default);

        Task<PackWMSResponse> CloseJl(CloseLuRequest request, CancellationToken cancellationToken = default);
    }
}