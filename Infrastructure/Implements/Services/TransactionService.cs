using Application.DTOs.Generic;
using Application.DTOs.Transaction;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using Domain.Enums.Transaction;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Utility;

namespace Infrastructure.Implements.Services
{
    public class TransactionService : GenericService<Transaction>, ITransactionService
    {
        private readonly IConfiguration config;
        public TransactionService(IOptionsSnapshot<AppConfig> configSnapshot,
                                  IUnitOfWork uow,
                                  ITimeService timeService,
                                  IConfiguration config,
                                  IClaimService claimService,
                                  ICacheService cacheService) : base(configSnapshot,
                                                                     uow,
                                                                     timeService,
                                                                     claimService,
                                                                     cacheService)
        {
            this.config = config;
        }
        #region Get transactions
        public IQueryable<Transaction> GetTransactions()
        {
            var source = uow.GetRepo<Transaction>().GetAll();
            var role = claimService.GetClaim(ClaimConstants.ROLE, Role.TRAVELER);
            switch (role)
            {
                case Role.TRAVELER:
                    var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                    return source.Where(t => t.AccountId == accountId);
            }
            return source;
        }
        #endregion
        #region Create top up
        public async Task<TopUpCreateResult> CreateTopUpAsync(TopUpCreate dto)
        {
            var transaction = new Transaction
            {
                CreatedAt = timeService.Now,
                Type = TransactionType.TOPUP,
                Status = TransactionStatus.PENDING,
                AccountId = claimService.GetClaim(ClaimConstants.ID, -1),
                Description = AppMessage.DESC_TRANSACTION_TOPUP,
                GcoinAmount = dto.Amount,
                Gateway = dto.Gateway
            };
            await uow.GetRepo<Transaction>().AddAsync(transaction);
            if (!await uow.SaveChangesAsync()) throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
            var response = new TopUpCreateResult
            {
                TransactionId = transaction.Id
            };
            switch (dto.Gateway)
            {
                case Gateway.VNPAY:
                    response.PaymentUrl = CreateVnpayTopUpRequest(transaction);
                    break;
            }
            return response;
        }
        private string CreateVnpayTopUpRequest(Transaction transaction)
        {
            string vnp_Returnurl = config["VNPAY:ReturnUrl"]!;
            string vnp_Url = config["VNPAY:Url"]!;
            string vnp_TmnCode = config["VNPAY:TmnCode"]!;
            string vnp_HashSecret = config["VNPAY:HashSecret"]!;
            string locale = config["VNPAY:Locale"]!;
            var conversionRate = GlobalConstants.VND_CONVERT_RATE;
            var expireTime = transaction.CreatedAt.AddMinutes(30).AddHours(7);//VNPAY nhận create & expire theo gmt +7
            VnPayLibrary vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            var vndAmount = transaction.GcoinAmount * conversionRate * 100;//VNPAY convert mặc định /100 lần
            vnpay.AddRequestData("vnp_Amount", vndAmount.ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnpay.AddRequestData("vnp_CreateDate", transaction.CreatedAt.AddHours(7).ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            var ipAddress = claimService.GetIpAddress();
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", locale);
            vnpay.AddRequestData("vnp_OrderInfo", string.Format("{0}", transaction.GcoinAmount));
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", transaction.Id.ToString());
            vnpay.AddRequestData("vnp_ExpireDate", expireTime.ToString("yyyyMMddHHmmss"));
            return vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        }
        #endregion
        #region Update top up result
        public async Task<Transaction> UpdateTopUpResultAsync(int transactionId, string bankTransCode, TransactionStatus status)
        {
            var transaction = await uow.GetRepo<Transaction>()
                                       .FindAsync(transactionId) ?? throw new KeyNotFoundException(AppMessage.ERR_TRANSACTION_NOT_FOUND);
            if (transaction.Account == null) throw new KeyNotFoundException(AppMessage.ERR_TRANSACTION_RECEIVER_NOT_FOUND);
            transaction.Status = status;
            transaction.BankTransCode = bankTransCode;
            transaction.Account.GcoinBalance += transaction.GcoinAmount;
            if (await uow.SaveChangesAsync())
            {
                transaction.Account.Transactions = [];
                return transaction;
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
    }
}
