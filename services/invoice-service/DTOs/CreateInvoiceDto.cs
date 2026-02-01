namespace InvoiceService.DTOs;

public class CreateInvoiceDto
{
    public Guid ProjectId { get; set; }
    public string InvoiceMonth { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
