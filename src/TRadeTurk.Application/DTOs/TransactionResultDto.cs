namespace TRadeTurk.Application.DTOs;

public class TransactionResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? TransactionId { get; set; }
    public decimal ExecutedPrice { get; set; }
    public decimal CommissionUsed { get; set; }
    public decimal SlippageAmount { get; set; }
}
