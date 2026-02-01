namespace ExpenseService.DTOs;

public class CreateExpenseDto
{
    public Guid WorkerId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string? Description { get; set; }
}
