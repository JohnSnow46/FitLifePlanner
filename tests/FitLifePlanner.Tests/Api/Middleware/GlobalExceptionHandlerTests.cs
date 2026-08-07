using FitLifePlanner.Api.Middleware;
using FitLifePlanner.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace FitLifePlanner.Tests.Api.Middleware;

public class GlobalExceptionHandlerTests
{
    [Theory]
    [InlineData(typeof(ValidationException), 400)]
    [InlineData(typeof(NotFoundException), 404)]
    public async Task TryHandleAsync_maps_known_exceptions_to_expected_status(Type exceptionType, int expectedStatus)
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, new FakeHostEnvironment());
        var exception = CreateException(exceptionType);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_maps_unknown_exception_to_500()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, new FakeHostEnvironment());
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(500, context.Response.StatusCode);
    }

    private static Exception CreateException(Type exceptionType) =>
        exceptionType == typeof(NotFoundException)
            ? new NotFoundException("User", 1)
            : (Exception)Activator.CreateInstance(exceptionType, "test message")!;

    private class FakeHostEnvironment : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "FitLifePlanner.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Production";
    }
}
