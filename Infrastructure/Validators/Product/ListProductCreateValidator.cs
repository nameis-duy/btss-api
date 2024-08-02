using Application.DTOs.Product;
using FluentValidation;

namespace Infrastructure.Validators.Product
{
    public class ListProductCreateValidator : AbstractValidator<List<ProductCreate>>
    {
        public ListProductCreateValidator(ProductCreateValidator childValidator)
        {
            RuleForEach(p => p).SetValidator(childValidator);
        }
    }
}
