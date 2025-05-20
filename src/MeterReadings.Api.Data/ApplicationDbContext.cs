using MeterReadings.Api.Data.Helpers;
using MeterReadings.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : DbContext(opts), IApplicationDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Check if we are in a Development environment
        // and if so, seed the database with the test accounts
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Development")
        {
            optionsBuilder.UseSeeding((context, _) =>
            {
                DbSeeder.SeedAccounts(context);
                context.SaveChanges();
            });
            optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                DbSeeder.SeedAccounts(context);
                await context.SaveChangesAsync(cancellationToken);
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<MeterReading>()
            .HasKey(m => m.Id);

        modelBuilder.Entity<MeterReading>()
            .HasOne(m => m.Account)
            .WithMany(a => a.MeterReadings)
            .HasForeignKey(m => m.AccountId);
    }

    // Tables
    public DbSet<Account> Accounts { get; set; }
    public DbSet<MeterReading> MeterReadings { get; set; }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }
}
