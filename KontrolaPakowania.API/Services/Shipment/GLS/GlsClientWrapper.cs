namespace KontrolaPakowania.API.Services.Shipment.GLS
{
    public class GlsClientWrapper : IGlsClientWrapper
    {
        private readonly Ade2PortTypeClient _client;

        public GlsClientWrapper(Ade2PortTypeClient client)
        {
            _client = client;
        }

        public Task<cSession> LoginAsync(string username, string password) =>
            _client.adeLoginAsync(username, password);

        public Task<cID> InsertParcelAsync(string sessionId, cConsign parcel) =>
            _client.adePreparingBox_InsertAsync(sessionId, parcel);

        public Task<adePreparingBox_GetConsignLabelsExtResponse> GetLabelsAsync(string sessionId, int parcelId, string format) =>
            _client.adePreparingBox_GetConsignLabelsExtAsync(sessionId, parcelId, format);

        public Task<cID> DeleteParcelAsync(string sessionId, int parcelId) =>
            _client.adePreparingBox_DeleteConsignAsync(sessionId, parcelId);

        public Task LogoutAsync(string sessionId) =>
            _client.adeLogoutAsync(sessionId);
    }
}