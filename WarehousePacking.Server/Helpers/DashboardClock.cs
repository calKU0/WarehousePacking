namespace WarehousePacking.Server.Helpers
{
    public static class DashboardClock
    {
        private static readonly object Sync = new();
        private static Timer? _timer;
        private static bool _started;

        public static DateTime Now { get; private set; } = DateTime.Now;

        public static event Action? Tick;

        public static void EnsureStarted()
        {
            if (_started)
            {
                return;
            }

            lock (Sync)
            {
                if (_started)
                {
                    return;
                }

                ScheduleNextTick();
                _started = true;
            }
        }

        private static void ScheduleNextTick()
        {
            var now = DateTime.Now;
            // Calculate exact milliseconds until the next full second
            var msToNextSecond = 1000 - now.Millisecond;

            // Use a one-shot timer (Timeout.Infinite) to prevent accumulated drift
            _timer?.Dispose();
            _timer = new Timer(_ => OnTick(), null, msToNextSecond, Timeout.Infinite);
        }

        private static void OnTick()
        {
            Now = DateTime.Now;
            Tick?.Invoke();


            ScheduleNextTick();
        }
    }
}