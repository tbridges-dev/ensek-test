using CsvHelper.Configuration;
using MeterReadings.Api.Models;

namespace MeterReadings.Api.Mappings;

public class MeterReadingReadMap : ClassMap<MeterReadingModel>
{
    public MeterReadingReadMap()
    {
        Map(x => x.AccountId).Name("AccountId");
        Map(x => x.MeterReadingDateTime).Name("MeterReadingDateTime")
            .TypeConverterOption.DateTimeStyles(System.Globalization.DateTimeStyles.AllowWhiteSpaces);
        Map(x => x.MeterReadValue).Name("MeterReadValue");
        Map(x => x.RowNumber).Convert(x => x.Row.Context.Parser?.Row ?? 0);
    }
}