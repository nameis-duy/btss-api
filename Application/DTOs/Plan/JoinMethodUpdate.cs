using Domain.Enums.Plan;

namespace Application.DTOs.Plan
{
    public class JoinMethodUpdate
    {
        public int PlanId { get; set; }
        public JoinMethod JoinMethod { get; set; }
    }
}
