using CsvHelper;
using CsvHelper.Configuration;
using MeterReadings.Api.Entities;
using MeterReadings.Api.Mappings;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MeterReadings.Api.Data.Helpers;

public class DbSeeder
{

    public static void SeedAccounts(DbContext context)
    {
        // Swapped to en-GB to match the format of the test data
        // var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("en-GB"))
        {
            MissingFieldFound = null
        };
        var csvFilePath = Path.Combine(AppContext.BaseDirectory, "SeedData", "Test_Accounts.csv");
        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, csvConfig);
        csv.Context.RegisterClassMap<AccountReadMap>();

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var record = csv.GetRecord<Account>();

            // Check if the record already exists in the database
            if (context.Set<Account>().Any(a => a.Id == record.Id))
                continue;

            context.Set<Account>().Add(record);
        }
    }
}
