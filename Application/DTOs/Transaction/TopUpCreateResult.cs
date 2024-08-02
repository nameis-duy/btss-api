namespace Application.DTOs.Transaction
{
#pragma warning disable CS8618
    public class TopUpCreateResult
    {
        public string PaymentUrl { get; set; }
        public int TransactionId { get; set; }
    }
}
