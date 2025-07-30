using System.Collections.Concurrent;
using System.Net;

namespace ConwayGameOfLife.Api.Middleware;

/// <summary>
/// Middleware for rate limiting requests
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _requestLimit;
    private readonly TimeSpan _timeWindow;
    private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requestStore = new();

    /// <summary>
    /// Creates a new instance of the rate limiting middleware
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">The logger</param>
    /// <param name="requestLimit">The maximum number of requests allowed in the time window</param>
    /// <param name="timeWindow">The time window in seconds</param>
    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        int requestLimit = 100,
        int timeWindow = 60)
    {
        _next = next;
        _logger = logger;
        _requestLimit = requestLimit;
        _timeWindow = TimeSpan.FromSeconds(timeWindow);
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var requestTime = DateTime.UtcNow;
        bool rateLimitExceeded = false;
        
        // Get or create a queue for this client
        var queue = _requestStore.GetOrAdd(clientIp, _ => new Queue<DateTime>());
        
        // Remove requests outside the time window
        lock (queue)
        {
            while (queue.Count > 0 && requestTime - queue.Peek() > _timeWindow)
            {
                queue.Dequeue();
            }
            
            // Check if the client has exceeded the request limit
            rateLimitExceeded = queue.Count >= _requestLimit;
            
            if (!rateLimitExceeded)
            {
                // Add the current request to the queue
                queue.Enqueue(requestTime);
            }
        }
        
        // Handle rate limit exceeded outside the lock
        if (rateLimitExceeded)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = _timeWindow.TotalSeconds.ToString();
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }
        
        // Continue to the next middleware
        await _next(context);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Try to get the IP from X-Forwarded-For header
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        
        // Fall back to the remote IP address
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}