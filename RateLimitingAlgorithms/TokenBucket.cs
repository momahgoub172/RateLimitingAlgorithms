/*
 * ***************************
 * Maintains a "bucket" that holds a fixed number of tokens.
 * Tokens are added to the bucket at a fixed rate (e.g., 1 token per second).
 * Each request consumes one token.
 * If no tokens are available, the request is denied (or queued, depending on the implementation).
 * The bucket has a maximum capacity, ensuring it doesn’t grow indefinitely.
 * ***************************
 * Token Bucket Example:
 *  - Bucket size: 10 tokens, refill rate: 1 token per second.
 *  - At 00:00:00: Bucket starts with 10 tokens → Client sends 10 requests → Allowed.
 *  - At 00:00:01: Only 1 token refilled → Client sends another 5 requests → Only 1 allowed, 4 denied.
 *      - because after time 00 bucket have 0 token
 *      - because after time 01 bucket have 1 token as refill rate is 1 token per second
 * **************************
 * The semaphore is a synchronization primitive that limits the number of threads that can access a resource
 * 
 * 
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimitingAlgorithms
{
    public class TokenBucket : IRateLimiter, IDisposable
    {
        private int _capacity;         // maximum number of tokens
        private int _currentTokens;    // current number of tokens
        private int _tokensPerSecond;  // refill rate
        private Timer _timer;          // timer for token refill
        private SemaphoreSlim _semaphore; // used to control access to the bucket

        public TokenBucket(int capacity, int tokensPerSecond)
        {
            _capacity = capacity;
            _currentTokens = capacity;
            _tokensPerSecond = tokensPerSecond;
            _semaphore = new SemaphoreSlim(_currentTokens, _capacity);


            double interval = 10000 / _tokensPerSecond;
            var refilInterval = (int)Math.Ceiling(interval);
            _timer = new Timer(RefillTokens, null, refilInterval, refilInterval);
        }

        private void RefillTokens(object? state)
        {
            int tokensCount = Interlocked.Increment(ref _currentTokens);
            if (tokensCount  >= _capacity)
            {
                Interlocked.Decrement(ref _currentTokens);
                try
                {
                    _semaphore.Release();
                    Console.WriteLine("Semaphore released");
                }
                catch (SemaphoreFullException)
                {
                    Console.WriteLine("Semaphore is full");
                }
            }
           
        }

        //acquire a token
        //0 indicates that the method does not wait if no token is available and returns false
        public bool AllowRequest()
        {
            return _semaphore.Wait(0);
        }

        public void Dispose()
        {
            _timer.Dispose();
            _semaphore.Dispose();
        }
    }
}
