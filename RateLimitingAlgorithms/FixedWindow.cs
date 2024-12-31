/*
 * **********************
 * Divides time into fixed intervals (windows), such as every second or every minute.
 * A fixed number of requests (limit) is allowed within each window.
 * At the start of each new window, the request count resets.
 * **********************
 * Fixed Window Example:
 *   - Limit: 10 requests per second.
 *   - At 00:00:00: Client sends 10 requests → Allowed.
 *   - At 00:00:01: Client sends another 10 requests → Allowed.
 * **********************
 * 
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace RateLimitingAlgorithms
{
    public class FixedWindow : IDisposable , IRateLimiter
    {
        private int _limit;        // maximum number of requests allowed
        private int _windowSize;   // in seconds
        private int _currentCount; // track the number of requests within the current window
        private Timer _timer;      // to reset the counter at the start of each new window
        private object _lock = new object();

        public FixedWindow(int limit, int windowSize)
        {
            _limit = limit;
            _windowSize = windowSize;
            _currentCount = 0;
            ScheduleTimer();
        }
        private void ScheduleTimer()
        {
            DateTime now = DateTime.Now;
            DateTime nextWindowStart = now.AddSeconds(_windowSize);
            // Calculate the remaining time until the start of the next window
            TimeSpan remainingTime = nextWindowStart - now;
            if (remainingTime <= TimeSpan.Zero)
                remainingTime = TimeSpan.Zero;
            /*
             * ResetCounter ==> is callback fired at the start of each new window
             * null ==> is the state passed to the callback
             * remainingTime ==> is the time remaining until the start of the next window
             * TimeSpan.FromSeconds ==> is the time interval between each callback means it calls every windowSize
             */
            _timer = new Timer(RestCounter, null, remainingTime, TimeSpan.FromSeconds(_windowSize));
        }

        private void RestCounter(object? state)
        {
            lock (_lock)
            {
                _currentCount = 0;
            }
            Console.WriteLine("counter reset , server can accept new request");
        }

        public bool AllowRequest()
        {
            /* 
            //simple but performace is poor
            //If another thread tries to enter a locked code, it will wait, block, until the object is released
            lock (_lock) {
                if (_currentCount < _limit)
                {
                    _currentCount++;
                    return true;
                }
                return false;
            }
            */

            //this approach is better(performace)
            if (Interlocked.Increment(ref _currentCount) > _limit)
            {
                Interlocked.Decrement(ref _currentCount);
                Console.WriteLine("current window is full");
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
