using RateLimitingAlgorithms;

//IRateLimiter rateLimiter = new FixedWindow(5, 10);
//IRateLimiter rateLimiter2 = new TokenBucket(5, 1);
//IRateLimiter rateLimiter3 = new SlidingWindow(5, TimeSpan.FromSeconds(10));
IRateLimiter rateLimiter4 = new LeakyBucket(5, 1);

WebServer webServer = new WebServer(8080, rateLimiter4);
await webServer.StartAsync();
