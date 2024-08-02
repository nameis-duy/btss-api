using Application.DTOs.Destination;
using Application.Interfaces.Services;
using FluentValidation;

namespace Infrastructure.Validators.Destination
{
    public class DestinationUpdateValidator : AbstractValidator<DestinationUpdate>
    {
        public DestinationUpdateValidator(IDestinationService destinationService,
                                          DestinationCreateValidator createValidator)
        {
            Include(createValidator);
            RuleFor(d => d.DestinationId).MustAsync(async (parent, destinationId, ct) =>
            {
                var destination = await destinationService.FindAsync(destinationId);
                return destination != null;
            }).WithMessage(AppMessage.ERR_DESTINATION_NOT_FOUND);
        }
    }
}
