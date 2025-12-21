using System.Collections.Concurrent;

namespace Api
{
    public class RateLimitService
    {
        private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitService(int maxRequests = 3, int timeWindowMinutes = 60)
        {
            _maxRequests = maxRequests;
            _timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
        }

        public bool IsRateLimited(string ipAddress)
        {
            var now = DateTime.UtcNow;

            // Get or create request list for this IP
            var requestTimes = _requests.GetOrAdd(ipAddress, _ => new List<DateTime>());

            lock (requestTimes)
            {
                // Remove old requests outside the time window
                requestTimes.RemoveAll(time => now - time > _timeWindow);

                // Check if limit exceeded
                if (requestTimes.Count >= _maxRequests)
                {
                    return true; // Rate limited
                }

                // Add current request
                requestTimes.Add(now);
                return false; // Not rate limited
            }
        }

        public void CleanupOldEntries()
        {
            var now = DateTime.UtcNow;
            var keysToRemove = new List<string>();

            foreach (var kvp in _requests)
            {
                lock (kvp.Value)
                {
                    kvp.Value.RemoveAll(time => now - time > _timeWindow);

                    if (kvp.Value.Count == 0)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                _requests.TryRemove(key, out _);
            }
        }
    }
}
