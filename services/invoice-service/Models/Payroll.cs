using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceService.Models;

[Table("payroll")]
public class Payroll
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("worker_id")]
    public Guid WorkerId { get; set; }

    [Column("payroll_month")]
    public string PayrollMonth { get; set; } = string.Empty;

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
