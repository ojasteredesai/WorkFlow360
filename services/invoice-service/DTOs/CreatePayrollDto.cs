namespace InvoiceService.DTOs;

public class CreatePayrollDto
{
    public Guid WorkerId { get; set; }
    public string PayrollMonth { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
