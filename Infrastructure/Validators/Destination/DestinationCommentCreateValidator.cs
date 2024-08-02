using Application.DTOs.Destination;
using Application.Interfaces.Services;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Destination
{
    public class DestinationCommentCreateValidator : AbstractValidator<DestinationCommentCreate>
    {
        public DestinationCommentCreateValidator(IDestinationService destinationService)
        {
            RuleFor(c => c.DestinationId).MustAsync(async (cmt, id, ct) =>
            {
                return await destinationService.GetAll().AnyAsync(d => d.Id == id && d.IsVisible, ct);
            }).WithMessage(AppMessage.ERR_DESTINATION_NOT_FOUND);
            RuleFor(c => c.Comment).Length(ValidationConstants.DESTINATION_COMMENT_MIN_LENGTH,
                                           ValidationConstants.DESTINATION_COMMENT_MAX_LENGTH)
                                   .WithMessage(string.Format(AppMessage.ERR_DESTINATION_COMMENT_LENGTH,
                                                              ValidationConstants.DESTINATION_COMMENT_MIN_LENGTH,
                                                              ValidationConstants.DESTINATION_COMMENT_MAX_LENGTH));
        }
    }
}
