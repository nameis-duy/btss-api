using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IBackgroundService
    {
        void RecalculateExecutionTime(TimeSpan additionalSpan);
        void SchedulePlanCancelNotify(int planId, DateTime enqueueAt);
        void SchedulePlanCancel(int planId, DateTime enqueueAt);
        void SchedulePlanDepartNotify(int planId, DateTime enqueueAt);
        void SchedulePlanVerifyNotify(int planId, DateTime enqueueAt);
        void SchedulePlanComplete(int planId, DateTime enqueueAt);
        void RecurPlanFinish();
        void ScheduleOrderFinish(int orderId, DateTime enqueueAt);
        void ScheduleProductUpdate(Product product, DateTime enqueueAt);
        void ScheduleRemoveDevice(int accountId, DateTime enqueueAt);
        void RecurCalculateDestinationTemp();
    }
}
