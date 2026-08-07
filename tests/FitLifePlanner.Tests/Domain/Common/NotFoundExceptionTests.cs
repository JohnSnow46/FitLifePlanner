using FitLifePlanner.Domain.Common;

namespace FitLifePlanner.Tests.Domain.Common;

public class NotFoundExceptionTests
{
    [Fact]
    public void Message_includes_entity_name_and_key()
    {
        var exception = new NotFoundException("User", 42);

        Assert.Equal("User with id '42' was not found.", exception.Message);
    }
}
