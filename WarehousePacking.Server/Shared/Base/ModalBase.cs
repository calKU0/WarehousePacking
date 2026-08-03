using Microsoft.AspNetCore.Components;

namespace WarehousePacking.Server.Shared.Base
{
    /// <summary>
    /// The shape every "ask the operator something" modal was hand-rolling: a
    /// visibility flag, a <see cref="TaskCompletionSource{TResult}"/> so the
    /// caller can simply await an answer, and open/close paths that resolve it
    /// exactly once.
    ///
    /// Deriving modals only describe their own content: set their fields, call
    /// <see cref="OpenAsync"/>, and answer with <see cref="Confirm"/> or
    /// <see cref="Cancel"/>.
    /// </summary>
    /// <typeparam name="TResult">What the caller gets back; null means cancelled.</typeparam>
    public abstract class ModalBase<TResult> : ComponentBase
    {
        private TaskCompletionSource<TResult?>? _pending;

        /// <summary>Bind this to AppModal's IsVisible.</summary>
        protected bool IsVisible { get; private set; }

        /// <summary>Shows the modal and hands the caller a task to await.</summary>
        protected Task<TResult?> OpenAsync()
        {
            // Reopening while someone is still waiting would strand that caller
            // for good, so the previous question is answered "cancelled" first.
            _pending?.TrySetResult(default);
            _pending = new TaskCompletionSource<TResult?>(TaskCreationOptions.RunContinuationsAsynchronously);

            IsVisible = true;
            Refresh();

            return _pending.Task;
        }

        /// <summary>Closes with an answer.</summary>
        protected void Confirm(TResult result) => Close(result);

        /// <summary>Closes without one — the caller receives null.</summary>
        protected void Cancel() => Close(default);

        private void Close(TResult? result)
        {
            if (!IsVisible && _pending is null) return;

            IsVisible = false;
            Refresh();

            var pending = _pending;
            _pending = null;
            pending?.TrySetResult(result);
        }

        /// <summary>
        /// Modals are closed from click handlers but also from timers and JS
        /// callbacks, so the re-render is always marshalled onto the renderer.
        /// </summary>
        private void Refresh() => _ = InvokeAsync(StateHasChanged);
    }
}
