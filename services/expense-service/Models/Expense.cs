using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseService.Models;

[Table("expenses")]
public class Expense
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("worker_id")]
    public Guid WorkerId { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("expense_date")]
    public DateOnly ExpenseDate { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    public string Status { get; set; } = "SUBMITTED";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
