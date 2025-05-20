using MeterReadings.Api.Data;
using MeterReadings.Api.Data.Helpers;
using MeterReadings.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Api.Tests;

public abstract class TestBase
{
    public IApplicationDbContext DbContext;
    // Accounts for unit tests
    public Account DefaultAccount1 = new()
    {
        Id = 1,
        FirstName = "Harrison",
        LastName = "Lesser"
    };
    public Account DefaultAccount2 = new()
    {
        Id = 2,
        FirstName = "Helen",
        LastName = "Troy"
    };

    protected TestBase()
    {
        // Use a unique name for the in-memory database to avoid conflicts
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("test_db_" + Guid.NewGuid().ToString())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;

        DbContext = new ApplicationDbContext(opts);
        // Seed the database with the test accounts
        DbSeeder.SeedAccounts((ApplicationDbContext)DbContext);
        DbContext.SaveAsync().Wait();
    }

    public async Task InitialiseDbAsync()
    {
        // Add the default accounts to the in-memory database if required
        DbContext.Accounts.AddRange(DefaultAccount1, DefaultAccount2);

        await DbContext.SaveAsync();
    }
}
