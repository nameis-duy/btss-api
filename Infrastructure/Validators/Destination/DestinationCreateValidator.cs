using Application.DTOs.Destination;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Destination
{
    public class DestinationCreateValidator : AbstractValidator<DestinationCreate>
    {
        public DestinationCreateValidator(IGenericService<Province> provinceService)
        {
            RuleFor(d => d.Name).Length(ValidationConstants.DESTINATION_NAME_MIN_LENGTH,
                                        ValidationConstants.DESTINATION_NAME_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_DESTINATION_NAME_LENGTH,
                                                           ValidationConstants.DESTINATION_NAME_MIN_LENGTH,
                                                           ValidationConstants.DESTINATION_NAME_MAX_LENGTH));
            RuleFor(d => d.Description).Length(ValidationConstants.DESTINATION_DESCRIPTION_MIN_LENGTH,
                                               ValidationConstants.DESTINATION_DESCRIPTION_MAX_LENGTH)
                                       .WithMessage(string.Format(AppMessage.ERR_DESTINATION_DESCRIPTION_LENGTH,
                                                                  ValidationConstants.DESTINATION_DESCRIPTION_MIN_LENGTH,
                                                                  ValidationConstants.DESTINATION_DESCRIPTION_MAX_LENGTH));
            RuleFor(d => d.ImageUrls).Count(ValidationConstants.DESTINATION_IMAGE_MIN, ValidationConstants.DESTINATION_IMAGE_MAX)
                                     .WithMessage(string.Format(AppMessage.ERR_IMAGE_COUNT,
                                                                ValidationConstants.DESTINATION_IMAGE_MIN,
                                                                ValidationConstants.DESTINATION_IMAGE_MAX))
                                     .AllFromValidSource()
                                     .WithMessage(AppMessage.ERR_IMAGE_SOURCE_INVALID);
            RuleFor(d => d.Address).Length(ValidationConstants.ADDRESS_MIN_LENGTH,
                                           ValidationConstants.ADDRESS_MAX_LENGTH)
                                   .WithMessage(string.Format(AppMessage.ERR_ADDRESS_LENGTH,
                                                              ValidationConstants.ADDRESS_MIN_LENGTH,
                                                              ValidationConstants.ADDRESS_MAX_LENGTH));
            RuleFor(d => d.Coordinate).IsInsideGeometry(ValidationConstants.REGION)
                                      .WithMessage(AppMessage.ERR_COORDINATE_INVALID);
            RuleFor(d => d.Seasons).AllValuesValid().WithMessage(AppMessage.ERR_DESTINATION_SEASON_INVALID);
            RuleFor(d => d.Topographic).IsInEnum().WithMessage(AppMessage.ERR_DESTINATION_TOPOGRAPHIC_INVALID);
            RuleFor(d => d.Activities).AllValuesValid().WithMessage(AppMessage.ERR_DESTINATION_ACTIVITY_INVALID);
            RuleFor(d => d.ProvinceId).MustAsync(async (provinceId, dest) => await provinceService.GetAll()
                                                                                                  .AnyAsync(p => p.Id == provinceId))
                                      .WithMessage(AppMessage.ERR_PROVINCE_NOT_FOUND);
        }
    }
}
