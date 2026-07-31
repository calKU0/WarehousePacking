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
    /// Multi-operator packing sessions: joining, snapshot sync and the
    /// main-operator rules.
    /// </summary>
    public partial class PackingPageBase
    {
        protected virtual async Task InitializeCollaborationSession()
        {
            if (PackageId <= 0)
                return;

            if (_sessionInitialized && _collaborationPackageId == PackageId)
                return;

            if (!_collaborationEventsSubscribed)
            {
                CollaborationService.SessionUpdated += OnCollaborationSessionUpdated;
                _collaborationEventsSubscribed = true;
            }

            var join = await CollaborationService.JoinSessionAsync(PackageId, UserSession.Username, JlItems, PackedItems);
            ApplySnapshot(join.Snapshot);

            IsMainOperator = join.IsMainOperator;
            _collaborationPackageId = join.Snapshot.PackageId;
            _sessionInitialized = true;

            if (!IsMainOperator)
                Toast.Show("Współdzielenie", $"Dołączono do pakowania. Główny operator: {MainOperator}.", ToastType.Info, 3500);

            await UpdateRealizationsPackageId();
            await InvokeAsync(StateHasChanged);
        }

        private async Task OnCollaborationSessionUpdated(PackingSessionEvent e)
        {
            if (!_sessionInitialized)
                return;

            var sessionMatch = e.PackageId == _collaborationPackageId || e.PreviousPackageId == _collaborationPackageId;
            if (!sessionMatch)
                return;

            if (e.EventType == PackingSessionEventType.PackageSwitched && e.PreviousPackageId == _collaborationPackageId)
            {
                _collaborationPackageId = e.PackageId;
                PackageId = e.PackageId;
                await UpdateRealizationsPackageId();
            }

            if (e.EventType == PackingSessionEventType.SessionClosed)
            {
                await InvokeAsync(() =>
                {
                    Toast.Show("Informacja", e.Message, ToastType.Info, 3500);
                    Navigation.NavigateTo("/kontrola-pakowania");
                });
                return;
            }

            var snapshot = CollaborationService.GetSnapshot(_collaborationPackageId);
            if (snapshot != null)
            {
                await InvokeAsync(() =>
                {
                    ApplySnapshot(snapshot);
                    if (e.EventType is PackingSessionEventType.OperatorJoined
                        or PackingSessionEventType.OperatorLeft
                        or PackingSessionEventType.MainOperatorChanged
                        or PackingSessionEventType.PackageSwitched)
                    {
                        Toast.Show("Współdzielenie", e.Message, ToastType.Info, 3500);
                    }
                    StateHasChanged();
                });
            }
        }

        protected void ApplySnapshot(PackingSessionSnapshot snapshot)
        {
            PackageId = snapshot.PackageId;
            MainOperator = snapshot.MainOperator;
            IsMainOperator = string.Equals(MainOperator, UserSession.Username, StringComparison.OrdinalIgnoreCase);
            ActiveOperators = snapshot.ActiveOperators;
            JlItems = snapshot.JlItems;
            PackedItems = snapshot.PackedItems;
            SelectedPackedItem = null;
        }

        protected bool EnsureMainOperator(string actionName)
        {
            if (IsMainOperator)
                return true;

            Toast.Show("Brak uprawnień", $"Tylko główny operator może wykonać: {actionName}.", ToastType.Error, 3500);
            return false;
        }

        protected async Task UpdateRealizationsPackageId()
        {
            await PackingService.UpdateJlRealization(new JlInProgressDto
            {
                Name = CurrentJl.Code,
                PackageId = PackageId
            });

            foreach (var jl in MergeJls)
            {
                await PackingService.UpdateJlRealization(new JlInProgressDto
                {
                    Name = jl.jlName,
                    PackageId = PackageId
                });
            }
        }
    }
}
