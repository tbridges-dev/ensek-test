using MeterReadings.Api.Models;

namespace MeterReadings.Api.Services;

public interface IMeterReadingService
{
    Task<ProcessMeterReadingsResultModel> ProcessMeterReadingsAsync(Stream csvStream, CancellationToken cancellationToken);
}
