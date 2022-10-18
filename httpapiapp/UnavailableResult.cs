namespace HttpApi;

public class UnavailableResult : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        httpContext.Response.Headers.Add("Retry-After", "10");
        return Task.CompletedTask;
    }
}