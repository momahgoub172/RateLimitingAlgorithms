/*
 * **********************
 * The bucket can hold a maximum number of requests (capacity).
 * If the bucket is full, new requests are rejected.
 * The bucket "leaks" (processes) requests at a constant rate (leakRate requests per second).
 * Requests are added to the bucket.
 * The bucket leaks requests at a fixed rate, reducing the number of requests in the bucket.
 * If the bucket is full, new requests are rejected.
 * **********************
 * Leaky Bucket Example:
 *   - Requests at 10:00:01, 10:00:02, 10:00:03, 10:00:04, and 10:00:05 are allowed because the bucket is not full.
 *   - 6th Request: The request at 10:00:06 is rejected because the bucket is full (5 requests).
 *   - The request at 10:00:07 is allowed because the bucket has leaked 2 requests (leak rate is 2 requests per second), reducing the count to 3.
 * **********************
 *   
 */







using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimitingAlgorithms
{
    public class LeakyBucket : IRateLimiter, IDisposable
    {
        private int _capacity;
        private int _leakyRate;
        private int _currentRequests;
        private DateTime _lastRequestTime;
        private object _lock = new object();



        public LeakyBucket(int capacity, int leakyRate)
        {
            _capacity = capacity;
            _leakyRate = leakyRate;
            _currentRequests = 0;
            _lastRequestTime = DateTime.UtcNow;
        }

        public bool AllowRequest()
        {
            lock (_lock)
            {
                // Leak requests based on the elapsed time since the last leak
                LeakRequests();
                //if the bucket is not full allow the request
                if (_currentRequests < _capacity)
                {
                    _currentRequests++;
                    _lastRequestTime = DateTime.UtcNow;
                    return true;
                }
                return false;
            }
        }



        private void LeakRequests()
        {
            DateTime now = DateTime.UtcNow;
            var elapsedTime = now - _lastRequestTime;

            /*
             * if elaspsedTime= 1 then requestsToLeak = 1 * _leakyRate
             * if elaspsedTime= 2 then requestsToLeak = 2 * _leakyRate
             * because every second we are leaking _leakyRate requestes
             */
            int requestsToLeak = (int)(elapsedTime.TotalSeconds * _leakyRate);

            if (requestsToLeak > 0)
            {
                Console.WriteLine("Leaking " + requestsToLeak + " requests");
                _currentRequests = Math.Max(0, _currentRequests - requestsToLeak);
                _lastRequestTime = now;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
