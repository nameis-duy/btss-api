using Application.DTOs.Provider;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Validators.Provider
{
    public class ListProviderCreateValidator : AbstractValidator<List<ProviderCreate>>
    {
        public ListProviderCreateValidator(ProviderCreateValidator childValidator)
        {
            RuleForEach(p => p).SetValidator(childValidator);
        }
    }
}
