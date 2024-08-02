using AppAny.HotChocolate.FluentValidation;
using Application.DTOs.Transaction;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Infrastructure.Validators.Transaction;

namespace API.GraphQL.Mutations
{
    public partial class Mutation
    {
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<TopUpCreateResult> CreateTopUpAsync([Service] ITransactionService transactionService,
                                                              [UseFluentValidation, UseValidator<TopUpCreateValidator>] TopUpCreate dto)
        {
            return await transactionService.CreateTopUpAsync(dto);
        }
    }
}
