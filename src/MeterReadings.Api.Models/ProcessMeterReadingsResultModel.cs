namespace MeterReadings.Api.Models;

public class ProcessMeterReadingsResultModel
{
    public int SuccessfulCount { get; set; }

    public int FailedCount { get; set; }

    public List<ProcessMeterReadingFailureModel> FailureMessages { get; set; } = [];
}

public class ProcessMeterReadingFailureModel
{
    public int CsvFileRow { get; set; }

    public int AccountId { get; set; }

    public DateTime MeterReadingDateTime { get; set; }

    public int MeterReadingValue { get; set; }

    public string FailureMessage { get; set; } = string.Empty;
}
