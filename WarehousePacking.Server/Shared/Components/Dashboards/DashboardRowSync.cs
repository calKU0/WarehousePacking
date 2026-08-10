namespace WarehousePacking.Server.Shared.Components.Dashboards
{
    /// <summary>
    /// One row of a dashboard table, with the animation state the wallboard needs.
    /// </summary>
    /// <remarks>
    /// <see cref="AnimationClass"/> is non-empty ONLY while a row is genuinely
    /// entering or leaving. A row whose data merely changed carries no class at
    /// all — the changed value animates itself (see DashboardAnimatedValue) and
    /// the row must stay put. Re-animating a whole table on every refresh is the
    /// exact behaviour this design avoids.
    /// </remarks>
    public sealed class DashboardRow<TData>
    {
        public required string Key { get; init; }

        public required TData Data { get; set; }

        public string AnimationClass { get; set; } = string.Empty;

        public bool IsLeaving { get; set; }
    }

    /// <summary>
    /// Keeps a live table in step with freshly fetched data, animating only the
    /// difference: a row that appears rises in, a row that disappears drops out,
    /// and every other row is left alone.
    /// </summary>
    /// <remarks>
    /// Every dashboard panel used to carry its own copy of this diff — six
    /// near-identical Sync/ClearEnter/Remove trios, each with its own animation
    /// timings. They are one implementation now, so a timing change lands
    /// everywhere at once and no panel can drift.
    ///
    /// Usage from a panel:
    /// <code>
    /// private readonly DashboardRowSync&lt;OperatorSummary&gt; _operators;
    /// public MyPanel() => _operators = new(() => InvokeAsync(StateHasChanged));
    /// // in OnParametersSet:
    /// _operators.Sync(BuildOperatorSummaries(), x =&gt; x.Operator);
    /// // in the markup: @foreach (var row in _operators.Rows)
    /// // and dispose the panel to stop pending animation callbacks.
    /// </code>
    /// </remarks>
    public sealed class DashboardRowSync<TData> : IDisposable
    {
        /// <summary>Must match `dashboard-row-enter` in css/pages/dashboard.css.</summary>
        public const int EnterDurationMs = 460;

        /// <summary>Must match `dashboard-row-leave` in css/pages/dashboard.css.</summary>
        public const int LeaveDurationMs = 300;

        private const string EnterClass = "dashboard-row-enter";
        private const string LeaveClass = "dashboard-row-leave";

        private readonly Func<Task> _notify;
        private readonly CancellationTokenSource _cts = new();

        public DashboardRowSync(Func<Task> notify)
        {
            _notify = notify;
        }

        /// <summary>
        /// Live rows in display order: the current data first, then any rows still
        /// playing their leave animation.
        /// </summary>
        public List<DashboardRow<TData>> Rows { get; private set; } = new();

        public bool HasRows => Rows.Count > 0;

        public void Sync(IReadOnlyList<TData> latest, Func<TData, string> keySelector)
        {
            var latestKeys = new HashSet<string>(latest.Select(keySelector), StringComparer.OrdinalIgnoreCase);

            foreach (var item in latest)
            {
                var key = keySelector(item);
                var existing = Rows.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

                if (existing is null)
                {
                    var added = new DashboardRow<TData>
                    {
                        Key = key,
                        Data = item,
                        AnimationClass = EnterClass
                    };

                    Rows.Add(added);
                    _ = ClearEnterClassAsync(added);
                    continue;
                }

                // The common case: the row is still here, only its values moved.
                // Updating Data in place leaves the row untouched in the DOM, so
                // nothing animates except the individual values that changed.
                existing.Data = item;

                if (existing.IsLeaving)
                {
                    // Came back before its exit finished — turn it around.
                    existing.IsLeaving = false;
                    existing.AnimationClass = EnterClass;
                    _ = ClearEnterClassAsync(existing);
                }
            }

            foreach (var row in Rows.Where(x => !x.IsLeaving && !latestKeys.Contains(x.Key)).ToList())
            {
                row.IsLeaving = true;
                row.AnimationClass = LeaveClass;
                _ = RemoveRowAsync(row);
            }

            // Re-order to match the incoming data, keeping the leaving rows pinned
            // at the end so they animate out without pushing live rows around.
            var byKey = Rows.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
            var ordered = new List<DashboardRow<TData>>(Rows.Count);
            var placed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in latest)
            {
                // `placed` guards against a key appearing twice in the incoming
                // data (a composite key that turns out not to be unique). Two
                // rows sharing an @key is a hard render error in Blazor, and a
                // wallboard must not die over a data quirk — the duplicate is
                // dropped instead.
                var key = keySelector(item);
                if (byKey.TryGetValue(key, out var row) && placed.Add(key))
                {
                    ordered.Add(row);
                }
            }

            ordered.AddRange(Rows.Where(x => x.IsLeaving));
            Rows = ordered;
        }

        /// <summary>Drops every row immediately, with no exit animation.</summary>
        public void Clear() => Rows = new List<DashboardRow<TData>>();

        private async Task ClearEnterClassAsync(DashboardRow<TData> row)
        {
            if (!await DelayAsync(EnterDurationMs) || row.IsLeaving)
            {
                return;
            }

            row.AnimationClass = string.Empty;
            await NotifyAsync();
        }

        private async Task RemoveRowAsync(DashboardRow<TData> row)
        {
            if (!await DelayAsync(LeaveDurationMs) || !row.IsLeaving)
            {
                return;
            }

            Rows = Rows.Where(x => !ReferenceEquals(x, row)).ToList();
            await NotifyAsync();
        }

        private async Task<bool> DelayAsync(int milliseconds)
        {
            try
            {
                await Task.Delay(milliseconds, _cts.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        private async Task NotifyAsync()
        {
            if (_cts.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await _notify();
            }
            catch (ObjectDisposedException)
            {
                // The panel went away between the delay and the re-render. A
                // wallboard must never surface that as an unhandled error.
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void Dispose()
        {
            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            _cts.Dispose();
        }
    }
}
