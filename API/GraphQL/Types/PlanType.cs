using Domain.Entities;
using Domain.JsonEntities;

namespace API.GraphQL.Types
{
    public class PlanType : ObjectType<Plan>
    {
        protected override void Configure(IObjectTypeDescriptor<Plan> descriptor)
        {
            descriptor.Field(p => p.Members).UseFiltering();
            descriptor.Field(p => p.Schedule).Type<JsonType>();
        }
    }
    
}
