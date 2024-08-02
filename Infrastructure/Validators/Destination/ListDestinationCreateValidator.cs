using Application.DTOs.Destination;
using FluentValidation;

namespace Infrastructure.Validators.Destination
{
    public class ListDestinationCreateValidator : AbstractValidator<List<DestinationCreate>>
    {
        public ListDestinationCreateValidator(DestinationCreateValidator validator)
        {
            RuleForEach(s => s).SetValidator(validator);
        }
    }
}
