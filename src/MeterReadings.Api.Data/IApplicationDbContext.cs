using MeterReadings.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Api.Data;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; set; }
    DbSet<MeterReading> MeterReadings { get; set; }

    Task SaveAsync(CancellationToken cancellationToken = default);
}
