using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MeterReadings.Api.Data;
using MeterReadings.Api.Entities;
using MeterReadings.Api.Mappings;
using MeterReadings.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeterReadings.Api.Services;

public class MeterReadingService(IApplicationDbContext dbContext, ILogger<MeterReadingService> logger) : IMeterReadingService
{
    private readonly ILogger<MeterReadingService> _logger = logger;
    private readonly IApplicationDbContext _dbContext = dbContext;
    
    private const int MeterReadingMinimum = 1;
    private const int MeterReadingMaximum = 99999;

    public async Task<ProcessMeterReadingsResultModel> ProcessMeterReadingsAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var processingOutput = new ProcessMeterReadingsResultModel();

        var processedReadings = new Dictionary<int, MeterReading>();

        // Changed to use en-GB culture to match the date format in the CSV file
        // var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture);
        var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("en-GB"))
        {
            MissingFieldFound = null
        };
        using var reader = new StreamReader(csvStream);
        using (var csv = new CsvReader(reader, csvConfig))
        {
            csv.Context.RegisterClassMap<MeterReadingReadMap>();

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var record = csv.GetRecord<MeterReadingModel>();

                var readingValidationResponse = await ValidateReading(record, processedReadings, cancellationToken);
                if (!string.IsNullOrEmpty(readingValidationResponse))
                {
                    processingOutput.FailedCount++;
                    processingOutput.FailureMessages.Add(new()
                    {
                        CsvFileRow = record.RowNumber,
                        AccountId = record.AccountId,
                        MeterReadingDateTime = record.MeterReadingDateTime,
                        MeterReadingValue = record.MeterReadValue,
                        FailureMessage = readingValidationResponse
                    });
                    continue;
                }
                else
                {
                    var reading = new MeterReading
                    {
                        AccountId = record.AccountId,
                        // Convert to store in UTC for consistency
                        DateTime = record.MeterReadingDateTime.ToUniversalTime(),
                        Value = record.MeterReadValue
                    };
                    _dbContext.MeterReadings.Add(reading);

                    // Try to add the success reading to the processed readings otherwise update the existing one
                    var previousAdded = processedReadings.TryAdd(record.AccountId, reading);
                    if (!previousAdded)
                        processedReadings[record.AccountId] = reading;

                    processingOutput.SuccessfulCount++;
                }
            }
        }

        if (processingOutput.SuccessfulCount > 0)
            await _dbContext.SaveAsync(cancellationToken);

        return processingOutput;
    }

    private async Task<string> ValidateReading(MeterReadingModel? meterReading, Dictionary<int, MeterReading> previousReadings, CancellationToken cancellationToken)
    {
        var response = string.Empty;

        // Check for empty record
        if (meterReading == null)
        {
            response = "Meter Reading record was empty.";
            _logger.LogWarning(response);
            return response;
        }

        // Validate reading AccountId
        if (meterReading.AccountId < 1)
        {
            response = $"Meter Reading AccountId was invalid. Received: {meterReading.AccountId}";
            _logger.LogWarning(response);
            return response;
        }

        // Validate that the Account of the reading exists
        if (!await _dbContext.Accounts.AnyAsync(x => x.Id == meterReading.AccountId, cancellationToken))
        {
            response = $"Account not found for Meter Reading Account Id. Received: {meterReading.AccountId}";
            _logger.LogWarning(response);
            return response;
        }

        // Get the most recent reading for the account, either from the cached readings, or failing that, the database
        var previousCustomerReading = previousReadings.FirstOrDefault(x => x.Key == meterReading.AccountId).Value
            ?? await _dbContext.MeterReadings.Where(x => x.AccountId == meterReading.AccountId).OrderByDescending(x => x.DateTime).FirstOrDefaultAsync(cancellationToken);

        // Validate that reading value is not below the minimum permitted value
        if (meterReading.MeterReadValue < MeterReadingMinimum)
        {
            response = $"Meter Reading Value ({meterReading.MeterReadValue}) is below the permitted minimum ({MeterReadingMinimum}).";
            _logger.LogWarning(response);
            return response;
        }

        // Validate that reading value is not below the most recent reading for the account
        if (previousCustomerReading is not default(MeterReading) && meterReading.MeterReadValue < previousCustomerReading.Value)
        {
            response = $"Meter Reading Value ({meterReading.MeterReadValue}) is less than the previous reading ({previousCustomerReading.Value})";
            _logger.LogWarning(response);
            return response;
        }

        // Validate that reading value is not above the maximum permitted value
        if (meterReading.MeterReadValue > MeterReadingMaximum)
        {
            response = $"Meter Reading Value ({meterReading.MeterReadValue}) is above the permitted maximum ({MeterReadingMaximum}).";
            _logger.LogWarning(response);
            return response;
        }

        // Validate that reading date is not equal or prior to most recent reading for the account
        if (previousCustomerReading is not default(MeterReading) && meterReading.MeterReadingDateTime.ToUniversalTime() <= previousCustomerReading.DateTime)
        {
            response = $"Meter Reading DateTime ({meterReading.MeterReadingDateTime:yyyy/MM/dd HH:mm:ss}) is the same or earlier than the most recent reading ({previousCustomerReading.DateTime:yyyy/MM/dd HH:mm:ss}).";
            _logger.LogWarning(response);
            return response;
        }

        return response;
    }
}
