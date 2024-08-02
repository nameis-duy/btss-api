using Application.DTOs.Transaction;
using Domain.Entities;
using Domain.Enums.Transaction;

namespace Application.Interfaces.Services
{
    public interface ITransactionService : IGenericService<Transaction>
    {
        Task<TopUpCreateResult> CreateTopUpAsync(TopUpCreate dto);
        IQueryable<Transaction> GetTransactions();
        Task<Transaction> UpdateTopUpResultAsync(int transactionId, string bankTransCode, TransactionStatus status);
    }
}
