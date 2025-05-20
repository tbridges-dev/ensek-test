using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeterReadings.Api.Entities;

public class MeterReading
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Required]
    public DateTime DateTime { get; set; }

    [Required]
    public int Value { get; set; }

    [ForeignKey(nameof(AccountId))]
    public virtual Account? Account { get; set; }
}
