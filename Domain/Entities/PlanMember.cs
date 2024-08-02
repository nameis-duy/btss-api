using Domain.Enums.Plan;

namespace Domain.Entities
{
#pragma warning disable CS8618
    public class PlanMember
    {
        public int Id { get; set; }
        public MemberStatus Status { get; set; }
        public int Weight { get; set; }
        public DateTime ModifiedAt { get; set; }
        //json
        public List<string>? Companions { get; set; }
        //many - one
        public int PlanId { get; set; }
        public Plan Plan { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public virtual HashSet<Transaction> Transactions { get; set;}
    }
}
