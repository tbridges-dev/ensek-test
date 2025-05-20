using CsvHelper;
using MeterReadings.Api.Models;
using MeterReadings.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.Globalization;

namespace MeterReadings.Api.Tests.Services;

public class MeterReadingServiceTests : TestBase
{
    private readonly MeterReadingService _sut;
    private readonly FakeLogger<MeterReadingService> _logger;
    private const int PermittedMinimumReadingValue = 1;
    private const int PermittedMaximumReadingValue = 99999;

    public MeterReadingServiceTests()
    {
        _logger = new FakeLogger<MeterReadingService>();
        _sut = new MeterReadingService(DbContext, _logger);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-99)]
    public async Task Returns_Failed_Count_When_Meter_Reading_AccountId_Is_Invalid(int accountId)
    {
        // Arrange
        await InitialiseDbAsync();
        var records = new List<MeterReadingModel> {
            new() {
                AccountId = accountId,
                MeterReadingDateTime = new DateTime(2001, 02, 03),
                MeterReadValue = 12345
            }
        };
        var stream = CreateCsvForProcessing(records);

        // Act
        var res = await _sut.ProcessMeterReadingsAsync(stream, CancellationToken.None);

        // Assert
        Assert.Equal(1, res.FailedCount);
        Assert.Equal(0, res.SuccessfulCount);
        Assert.Equal(0, DbContext.MeterReadings.Count());
        Assert.Equal(1, _logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, _logger.Collector.GetSnapshot()[0].Level);
        Assert.Equal($"Meter Reading AccountId was invalid. Received: {accountId}", _logger.Collector.GetSnapshot()[0].Message);
    }

    [Fact]
    public async Task Returns_Failed_Count_When_Meter_Reading_AccountId_Does_Not_Exist()
    {
        // Arrange
        await InitialiseDbAsync();
        var nonExistantAccountId = 999;
        var records = new List<MeterReadingModel> {
            new() {
                AccountId = nonExistantAccountId,
                MeterReadingDateTime = new DateTime(2001, 02, 03),
                MeterReadValue = 12345
            }
        };
        var stream = CreateCsvForProcessing(records);

        // Act
        var res = await _sut.ProcessMeterReadingsAsync(stream, CancellationToken.None);

        // Assert
        Assert.Equal(1, res.FailedCount);
        Assert.Equal(0, res.SuccessfulCount);
        Assert.Equal(0, DbContext.MeterReadings.Count());
        Assert.Equal(1, _logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, _logger.Collector.GetSnapshot()[0].Level);
        Assert.Equal($"Account not found for Meter Reading Account Id. Received: {nonExistantAccountId}", _logger.Collector.GetSnapshot()[0].Message);
    }

    [Theory]
    [InlineData("2001, 02, 03", "2001, 02, 02")]
    [InlineData("2001, 02, 03", "2001, 02, 03")]
    public async Task Returns_Failed_Count_When_Meter_Reading_Date_Time_Is_Equal_To_Or_Earlier_Than_Existing_Reading(string previousReadingDate, string newReadingDate)
    {
        // Arrange
        await InitialiseDbAsync();
        var previousReadingDateTime = DateTime.Parse(previousReadingDate);
        DbContext.MeterReadings.Add(new Entities.MeterReading
        {
            Id = 1,
            AccountId = DefaultAccount1.Id,
            DateTime = previousReadingDateTime,
            Value = 12345
        });
        await DbContext.SaveAsync(TestContext.Current.CancellationToken);

        var newReadingDateTime = DateTime.Parse(newReadingDate);
        var records = new List<MeterReadingModel> {
            new() {
                AccountId = DefaultAccount1.Id,
                MeterReadingDateTime = newReadingDateTime,
                MeterReadValue = 23456
            }
        };
        var stream = CreateCsvForProcessing(records);

        // Act
        var res = await _sut.ProcessMeterReadingsAsync(stream, CancellationToken.None);

        // Assert
        Assert.Equal(1, res.FailedCount);
        Assert.Equal(0, res.SuccessfulCount);
        Assert.Equal(1, DbContext.MeterReadings.Count());
        Assert.Equal(1, _logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, _logger.Collector.GetSnapshot()[0].Level);
        Assert.Equal($"Meter Reading DateTime ({newReadingDateTime:yyyy/MM/dd HH:mm:ss}) is the same or earlier than the most recent reading ({previousReadingDateTime:yyyy/MM/dd HH:mm:ss}).", _logger.Collector.GetSnapshot()[0].Message);
    }

    [Theory]
    [InlineData("2001, 02, 03", "2001, 02, 02")]
    [InlineData("2001, 02, 03", "2001, 02, 03")]
    public async Task Returns_Failed_Count_When_Meter_Reading_Date_Time_Is_Equal_To_Or_Earlier_Than_Previous_Reading_In_Same_File(string previousReadingDate, string newReadingDate)
    {
        // Arrange
        await InitialiseDbAsync();
        var previousReadingDateTime = DateTime.Parse(previousReadingDate);
        var newReadingDateTime = DateTime.Parse(newReadingDate);
        var records = new List<MeterReadingModel> {
            new() {
                AccountId = DefaultAccount1.Id,
                MeterReadingDateTime = previousReadingDateTime,
                MeterReadValue = 12345
            },
            new() {
                AccountId = DefaultAccount1.Id,
                MeterReadingDateTime = newReadingDateTime,
                MeterReadValue = 23456
            }
        };
        var stream = CreateCsvForProcessing(records);

        // Act
        var res = await _sut.ProcessMeterReadingsAsync(stream, CancellationToken.None);

        // Assert
        Assert.Equal(1, res.FailedCount);
        Assert.Equal(1, res.SuccessfulCount);
        Assert.Equal(1, DbContext.MeterReadings.Count());
        Assert.Equal(1, _logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, _logger.Collector.GetSnapshot()[0].Level);
        Assert.Equal($"Meter Reading DateTime ({newReadingDateTime:yyyy/MM/dd HH:mm:ss}) is the same or earlier than the most recent reading ({previousReadingDateTime:yyyy/MM/dd HH:mm:ss}).", _logger.Collector.GetSnapshot()[0].Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-12345)]
    public async Task Returns_Failed_Count_When_Meter_Reading_Value_Is_Below_Minimum(int readingValue)
    {
        // Arrange
        await InitialiseDbAsync();
        var records = new List<MeterReadingModel> {
            new() {
                AccountId = DefaultAccount1.Id,
                MeterReadingDateTime = new DateTime(2001, 02, 03),
                MeterReadValue = readingValue
            }
        };
        var stream = CreateCsvForProcessing(records);

        // Act
        var res = await _sut.ProcessMeterReadingsAsync(stream, CancellationToken.None);

        // Assert
        Assert.Equal(1, res.FailedCount);
        Assert.Equal(0, res.SuccessfulCount);
        Assert.Equal(0, DbContext.MeterReadings.Count());
        Assert.Equal(1, _logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, _logger.Collector.GetSnapshot()[0].Level);
        Assert.Equal($"Meter Reading Value ({readingValue}) is below the permitted minimum ({PermittedMinimumReadingValue}).", _logger.Collector.GetSnapshot()[0].Message);
    }

    [Theory]
    [InlineData(100000)]
    [InlineData(1234567890)]
    public async Task Returns_Failed_Count_When_Meter_Reading_Value_Is_Above_Maximum(int readingValue)
    {
        // Arrange
        await InitialiseDbAsync();
        var records = new List<MeterReadingModel> {
            new() {
                AccountId = DefaultAccount1.Id,
                MeterReadingDateTime = new DateTime(2001, 02, 03),
                MeterReadValue = readingValue
            }
        };
        var stream = CreateCsvForProcessing(records);

        // Act
        var res = await _sut.ProcessMeterReadingsAsync(stream, CancellationToken.None);

        // Assert
        Assert.Equal(1, res.FailedCount);
        Assert.Equal(0, res.SuccessfulCount);
        Assert.Equal(0, DbContext.MeterReadings.Count());
        Assert.Equal(1, _logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, _logger.Collector.GetSnapshot()[0].Level);
        Assert.Equal($"Meter Reading Value ({readingValue}) is above the permitted maximum ({PermittedMaximumReadingValue}).", _logger.Collector.GetSnapshot()[0].Message);
    }

    [Fact]
    public async Task Adds_Meter_Readings_To_Db_When_Meter_Readings_Are_Valid()
    {
        // Arrange
        await InitialiseDbAsync();
        var records = new List<MeterReadingModel>
        {
            new() {
                AccountId = DefaultAccount1.Id,
                MeterReadingDateTime = new DateTime(2001, 02, 03, 00, 00, 00),
                MeterReadValue = 12345
            },
            new() {
                AccountId = DefaultAccount2.Id,
                MeterReadingDateTime = new DateTime(2002, 03, 04, 05, 06, 07),
                MeterReadValue = 23456
            },
            new() {
                AccountId = DefaultAccount1.Id,
                MeterReadingDateTime = new DateTime(2001, 02, 03, 00, 00, 01),
                MeterReadValue = 34567
            }
        };
        var stream = CreateCsvForProcessing(records);

        // Act
        var res = await _sut.ProcessMeterReadingsAsync(stream, CancellationToken.None);

        // Assert
        Assert.Equal(0, res.FailedCount);
        Assert.Equal(3, res.SuccessfulCount);
        Assert.Contains(DbContext.MeterReadings.Where(x => x.AccountId == 1), (r) => r.DateTime == records[0].MeterReadingDateTime && r.Value == records[0].MeterReadValue);
        Assert.Contains(DbContext.MeterReadings.Where(x => x.AccountId == 2), (r) => r.DateTime == records[1].MeterReadingDateTime && r.Value == records[1].MeterReadValue);
        Assert.Contains(DbContext.MeterReadings.Where(x => x.AccountId == 1), (r) => r.DateTime == records[2].MeterReadingDateTime && r.Value == records[2].MeterReadValue);
    }

    [Fact]
    public async Task Processes_Test_Data_As_Expected_With_Fresh_Database()
    {
        // Arrange
        await InitialiseDbAsync();

        var csvFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "Meter_Reading.csv");
        using var reader = new StreamReader(csvFilePath);
        // Act
        var res = await _sut.ProcessMeterReadingsAsync(reader.BaseStream, CancellationToken.None);

        // Assert
        Assert.Equal(24, res.SuccessfulCount);
        Assert.Equal(11, res.FailedCount);
    }

    private static MemoryStream CreateCsvForProcessing(List<MeterReadingModel> records)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        // using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
        using var csv = new CsvWriter(writer, CultureInfo.GetCultureInfo("en-GB"));
        csv.WriteRecords(records);

        csv.Flush();
        stream.Position = 0;

        return stream;
    }
}
