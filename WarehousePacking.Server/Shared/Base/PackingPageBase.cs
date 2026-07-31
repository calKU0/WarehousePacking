using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Server.Helpers;
using WarehousePacking.Server.Services;
using WarehousePacking.Server.Settings;
using WarehousePacking.Server.Shared.Components;
using WarehousePacking.Server.Shared.Components.Modals;
using WarehousePacking.Server.Shared.Components.Packing;

namespace WarehousePacking.Server.Shared.Base
{
    /// <summary>
    /// Core state and component lifecycle for both packing screens.
    ///
    /// The rest of the behaviour lives in partial files alongside this one:
    /// Collaboration, Data, Packing, Finishing and Manager.
    /// </summary>
    public partial class PackingPageBase : ComponentBase, IDisposable
    {
        [Inject] protected PackingService PackingService { get; set; } = null!;
        [Inject] protected WorkstationService WorkstationService { get; set; } = null!;
        [Inject] protected UserSessionService UserSession { get; set; } = null!;
        [Inject] protected AuthService AuthService { get; set; } = null!;
        [Inject] protected ShipmentService ShipmentService { get; set; } = null!;
        [Inject] protected ClientPrinterService ClientPrinterService { get; set; } = null!;
        [Inject] protected NavigationManager Navigation { get; set; } = null!;
        [Inject] protected PackingCollaborationService CollaborationService { get; set; } = null!;

        // Parameters
        [Parameter] public string Jl { get; set; } = string.Empty;

        // Shared state
        protected JlData CurrentJl = new();

        protected List<(string jlName, string locationCode)> MergeJls = new();
        protected List<JlItemDto> JlItems = new();
        protected List<JlItemDto> PackedItems = new();
        protected HashSet<string> HighlightedRows = new();
        protected WorkstationSettings Settings = new();
        protected CourierConfiguration CourierConfiguration = new();
        protected int PackageId;

        // Modals & Toasts
        protected Toast Toast = new();

        protected ProductSelectModal ProductSelectModal = new();
        protected PasswordModal PasswordModal = new();
        protected ConfirmDialog ConfirmDialog = new();
        protected ManagerControlModal ManagerModal = new();
        protected TextBoxModal TextBoxModal = new();
        protected CourierModal CourierModal = new();
        protected LoggedOperatorsModal LoggedOperatorsModal = new();
        protected JlInProgressModal JlInProgressModal = new();
        protected ChangePackingWarehouseModal ChangePackingWarehouseModal = new();
        protected DimensionsModal DimensionsModal = new();
        protected FinishPackingModal FinishPackingModal = new();
        protected ShipmentModal ShipmentModal = new();
        protected ScanInput ScanInputComponent = new();
        protected JlSelectModal JlSelectModal = new();
        protected MergePackagesModal MergePackagesModal = new();
        protected CourierConfigurationModal CourierConfigurationModal = new();

        protected JlItemDto? SelectedItem;
        protected JlItemDto? SelectedPackedItem;

        protected PackingFlow _currentPackingFlow;
        protected string InternalBarcodeTemp = string.Empty;
        protected bool PackingToBufor = false;
        protected bool IsInitializingLoading { get; set; }
        protected bool IsFinishingLoading { get; set; }
        protected bool IsPageLoading => IsInitializingLoading || IsFinishingLoading;
        protected bool IsMainOperator = true;
        protected string MainOperator = string.Empty;
        protected List<string> ActiveOperators = new();
        private int _collaborationPackageId;
        private bool _sessionInitialized;
        private bool _collaborationEventsSubscribed;
        private bool _locationCleanupDone;
        private int _lastPackStockPackageId;
        private string _lastPackStockPackageCode = string.Empty;
        private readonly HashSet<string> _closingWmsPackages = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _closedWmsPackages = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task OnInitializedAsync()
        {
            IsInitializingLoading = true;
            try
            {
                await UserSession.InitializeAsync();
                await LoadSettings();
                LoadMergeJlsFromQuery();
                await LoadJlData();
                await AddJlRealizations();

                Navigation.LocationChanged += OnLocationChanged;

                await ShowPackingRequirements();
                await CheckRouteStatus();
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy inicjalizacji: {ex.Message}");
            }
            finally
            {
                IsInitializingLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await ScanInputComponent.FocusAsync();
            }
        }

        protected virtual async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            var uri = Navigation.ToBaseRelativePath(Navigation.Uri);
            if (!uri.StartsWith("kontrola-pakowania/", StringComparison.OrdinalIgnoreCase) && !_locationCleanupDone)
            {
                await CollaborationService.LeaveSessionAsync(PackageId, UserSession.Username, closeSession: false);
                _locationCleanupDone = true;
            }
        }

        public virtual void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
            CollaborationService.SessionUpdated -= OnCollaborationSessionUpdated;
        }
    }
}
