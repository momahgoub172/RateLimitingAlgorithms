/*
 * **********************
 * Define a fixed time window (e.g., 60 seconds) during which requests are counted
 * Maintain a list of timestamps for incoming requests.
 * As time progresses, the window "slides" forward, and requests older than the window size are discarded
 * For each new request:
       - Remove timestamps older than the current window.
       - If the number of requests within the window is below the limit, allow the request and add its timestamp to the list.
       - If the limit is exceeded, reject the request.
* **********************
 * Sliding Window Example:
 *    - Rate Limit: 5 requests per 10 seconds.
 *    - Request Timestamps: [10:00:01, 10:00:03, 10:00:05, 10:00:07, 10:00:09]
 *    - New Request at 10:00:10:
 *    - Remove timestamps older than 10:00:00 (none in this case).
 *    - Count of requests within the window: 5.
 *    - Since the count equals the limit, the request is rejected.
 * Another Example:
 *    - Rate Limit: 5 requests per 10 seconds.
 *    - Request Timestamps: [10:00:01, 10:00:03, 10:00:05, 10:00:07, 10:00:09]
 *    - New Request at 10:00:11:
 *    - Remove timestamps older than 10:00:01 (10:00:01 is removed)  --> 10:00:01 (10:00:11 - 10 seconds)
 *    - Updated Request Timestamps: [10:00:03, 10:00:05, 10:00:07, 10:00:09]
 *    - Count of requests within the window: 4.
 *    - Since the count is below the limit, the request is allowed.
 *    - Updated Request Timestamps: [10:00:03, 10:00:05, 10:00:07, 10:00:09, 10:00:11]
 * ********************
 * 
 */



using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimitingAlgorithms
{
    public class SlidingWindow : IRateLimiter, IDisposable
    {
        private int _limit;
        private TimeSpan _windowSize;
        private Queue<DateTime> _queue;
        private object _lock = new object();

        public SlidingWindow(int limit, TimeSpan windowSize)
        {
            _limit = limit;
            _windowSize = windowSize;
            _queue = new Queue<DateTime>();
        }




        public bool AllowRequest()
        {
            lock (_lock) 
            {
                DateTime now = DateTime.Now;
                while (_queue.Count > 0 && now - _queue.Peek() > _windowSize)
                {
                    _queue.Dequeue();
                }

                if(_queue.Count < _limit) 
                {
                    _queue.Enqueue(now);
                    return true;
                }
                return false;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
