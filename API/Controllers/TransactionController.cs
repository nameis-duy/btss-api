using API.GraphQL.Subscriptions;
using Application.Interfaces.Services;
using Domain.Enums.Transaction;
using HotChocolate.Subscriptions;
using Infrastructure;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utility;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService transactionService;
        public const string TOPUP_TOPIC_FORMAT = $"{{0}}_{nameof(Subscription.TopUpStatus)}";
        public TransactionController(ITransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

        [HttpGet("webhook/VnPayIPN")]
        public async Task<IActionResult> FinishVNPAYTopUpAsync([FromServices] IConfiguration config,
                                                               [FromServices] ITopicEventSender sender)
        {
            if (HttpContext.Request.Query.Any())
            {
                string vnp_HashSecret = config["VNPAY:HashSecret"]!;
                var vnpayData = Request.Query;
                VnPayLibrary vnpay = new VnPayLibrary();
                foreach (string s in vnpayData.Keys)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]!);
                    }
                }
                //Lay danh sach tham so tra ve tu VNPAY
                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                //vnp_TransactionNo: Ma GD tai he thong VNPAY
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                //vnp_SecureHash: HmacSHA512 cua du lieu tra ve
                int transactionId = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                string vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_SecureHash = Request.Query["vnp_SecureHash"]!;
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    var transaction = await transactionService.GetAll(true)
                                                              .Include(t => t.Account)
                                                              .FirstOrDefaultAsync(t => t.Id == transactionId);
                    if (transaction == null) 
                        return NotFound(new { RspCode = "01", Message = AppMessage.ERR_TRANSACTION_NOT_FOUND });
                    if (transaction.GcoinAmount != vnp_Amount / GlobalConstants.VND_CONVERT_RATE)
                        return Conflict(new { RspCode = "04", Message = AppMessage.ERR_TRANSACTION_VNPAY_AMOUNT });
                    if (transaction.Status != TransactionStatus.PENDING) 
                        return Conflict(new { RspCode = "02", Message = AppMessage.ERR_TRANSACTION_VNPAY_STATUS });
                    var status = TransactionStatus.ERROR;
                    try
                    {
                        if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                        {
                            status = TransactionStatus.ACCEPTED;
                            transaction = await transactionService.UpdateTopUpResultAsync(transactionId, vnpayTranId, status);
                            var topic = string.Format(TOPUP_TOPIC_FORMAT, transactionId);
                            await sender.SendAsync(topic, transaction);
                            return Ok(new { RspCode = "00", Message = AppMessage.SUC_TRANSACTION_VNPAY });
                        }
                    }
                    catch
                    {
                        return NotFound(new { RspCode = "99", Message = AppMessage.ERR_FUNCTION_DOWN });
                    }
                }
                return Conflict(new { RspCode = "97", Message = AppMessage.ERR_TRANSACTION_VNPAY_SIGNATURE });
            }
            return Conflict(new { RspCode = "99", Message = AppMessage.ERR_TRANSACTION_VNPAY_EMPTY });
        }
    }
}
