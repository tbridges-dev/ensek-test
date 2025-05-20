using System.ComponentModel.DataAnnotations;

namespace MeterReadings.Api.Models;

public class MeterReadingModel
{
    [Required]
    public int AccountId { get; set; }

    [Required]
    public DateTime MeterReadingDateTime { get; set; }

    [Required]
    public int MeterReadValue { get; set; }

    public int RowNumber { get; set; }
}