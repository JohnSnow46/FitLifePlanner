using FitLifePlanner.Domain.Users;
using FitLifePlanner.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FitLifePlanner.Tests.Infrastructure;

public class FitLifePlannerDbContextTests
{
    [Fact]
    public void SaveChanges_persists_user_and_reads_it_back()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<FitLifePlannerDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new FitLifePlannerDbContext(options);
        context.Database.EnsureCreated();

        context.Users.Add(new User { Name = "Jan Kowalski", Email = "jan.kowalski@example.com" });
        context.SaveChanges();

        var savedUser = context.Users.Single();

        Assert.Equal("Jan Kowalski", savedUser.Name);
        Assert.Equal("jan.kowalski@example.com", savedUser.Email);
    }
}
