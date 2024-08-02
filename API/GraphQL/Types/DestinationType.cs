using Domain.Entities;

namespace API.GraphQL.Types
{
    public class DestinationType : ObjectType<Destination>
    {
        protected override void Configure(IObjectTypeDescriptor<Destination> descriptor)
        {
            descriptor.Field(d => d.UnaccentName).Ignore();
            descriptor.Field(d => d.NameVector).Ignore();
        }
    }
}
