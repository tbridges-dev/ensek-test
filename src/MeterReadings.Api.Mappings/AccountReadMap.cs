using CsvHelper.Configuration;
using MeterReadings.Api.Entities;

namespace MeterReadings.Api.Mappings;

public class AccountReadMap : ClassMap<Account>
{
    public AccountReadMap()
    {
        Map(x => x.Id).Name("AccountId");
        Map(x => x.FirstName).Name("FirstName");
        Map(x => x.LastName).Name("LastName");
        Map(x => x.MeterReadings).Ignore();
    }
}