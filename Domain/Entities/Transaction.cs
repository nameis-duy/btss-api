using Domain.Enums.Transaction;
using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Entities
{
#pragma warning disable CS8618
    public class Transaction
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal GcoinAmount { get; set; }
        public string Description { get; set; }
        public Gateway Gateway { get; set; }
        public string? BankTransCode { get; set; }
        public DateTime CreatedAt { get; set; }
        //many - one
        public int? AccountId { get; set; }
        public virtual Account? Account { get; set; }
        public int? ProviderId { get; set; }
        public virtual Provider? Provider { get; set; }
        public int? PlanMemberId { get; set; }
        public virtual PlanMember? PlanMember { get; set; }
        public int? OrderId { get; set; }
        public virtual Order? Order { get; set; }
    }
}
