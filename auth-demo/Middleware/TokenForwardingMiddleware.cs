namespace AuthDemo.Middleware;

public class TokenForwardingMiddleware
{
    private readonly RequestDelegate _next;

    public TokenForwardingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
        {
            // Add the token to the Authorization header if it's not already present
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Request.Headers.Append("Authorization", $"Bearer {token}");
            }
        }

        await _next(context);
    }
}