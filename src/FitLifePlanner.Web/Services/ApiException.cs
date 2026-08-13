using System.Net;

namespace FitLifePlanner.Web.Services;

public class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
