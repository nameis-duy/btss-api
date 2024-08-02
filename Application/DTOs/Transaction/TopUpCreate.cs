using Domain.Enums.Transaction;

namespace Application.DTOs.Transaction
{
    public class TopUpCreate
    {
        public long Amount { get; set; }
        public Gateway Gateway { get; set; }
    }
    
}
