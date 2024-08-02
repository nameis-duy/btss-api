using Application.DTOs.Generic;
using Application.DTOs.Staff_Admin;
using Application.DTOs.Traveler;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;
using Vonage;
using Vonage.Verify;

namespace Infrastructure.Implements.Services
{
    public class AccountService : GenericService<Account>, IAccountService
    {
        private readonly IConfiguration config;
        private readonly VonageClient vonageClient;
        private readonly IBackgroundService backgroundService;
        public AccountService(IOptionsSnapshot<AppConfig> configSnapshot,
                              IUnitOfWork uow,
                              ITimeService timeService,
                              IClaimService claimService,
                              ICacheService cacheService,
                              IConfiguration config,
                              VonageClient vonageClient,
                              IBackgroundService backgroundService) : base(configSnapshot,
                                                                 uow,
                                                                 timeService,
                                                                 claimService,
                                                                 cacheService)
        {
            this.config = config;
            this.vonageClient = vonageClient;
            this.backgroundService = backgroundService;
        }
        #region private: Generate access & refresh token
        private AuthResult GenerateAuthorizeTokens(IEnumerable<Claim> claims, DateTime? now = null)
        {
            now ??= timeService.Now;
            var refreshToken = JwtUtil.GenerateJWT(claims,
                                                   now.Value,
                                                   AuthConstants.REFRESH_TOKEN_LIFETIME_MINUTE,
                                                   config,
                                                   AuthConstants.REFRESH_USE_HMACSHA512,
                                                   config["Jwt:RefreshKey"]);
            var accessToken = JwtUtil.GenerateJWT(claims,
                                                  now.Value,
                                                  AuthConstants.ACCESS_TOKEN_LIFETIME_MINUTE,
                                                  config);
            return new AuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        #endregion
        #region Get Account
        public IQueryable<Account> GetAccounts()
        {
            var source = uow.GetRepo<Account>().GetAll();
            var role = claimService.GetClaim(ClaimConstants.ROLE, Role.TRAVELER);
            switch (role)
            {
                case Role.ADMIN:
                    return source;
                case Role.PROVIDER:
                case Role.STAFF:
                    var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                    return source.Where(a => a.Id == accountId);
            }
            return source.Where(a => a.Role == Role.TRAVELER);
        }
        #endregion
        #region Refresh auth
        public async Task<AuthResult> RefreshAuthAsync(string refreshToken)
        {
            var isValidToken = JwtUtil.VerifyJWT(refreshToken,
                                                        config,
                                                        out IEnumerable<Claim> claims,
                                                        AuthConstants.REFRESH_USE_HMACSHA512,
                                                        config["Jwt:RefreshKey"]);
            if (!isValidToken) throw new UnauthorizedAccessException(AppMessage.ERR_TOKEN_INVALID);
            var role = claimService.GetClaim(ClaimConstants.ROLE, Role.UNKNOWN);
            switch (role)
            {
                case Role.PROVIDER:
                case Role.STAFF:
                case Role.TRAVELER:
                    var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                    var account = await uow.GetRepo<Account>().FindAsync(accountId);
                    if (account == null || !account.IsActive) 
                        throw new UnauthorizedAccessException(AppMessage.ERR_AUTHORIZE);
                    if (account.Role == Role.TRAVELER)
                        backgroundService.ScheduleRemoveDevice(accountId,
                                                               timeService.Now.AddMinutes(AuthConstants.REFRESH_TOKEN_LIFETIME_MINUTE));
                    break;
            }
            return GenerateAuthorizeTokens(claims);
        }
        #endregion
        #region Traveler request OTP
        public async Task<bool> TravelerRequestOTPAsync(TravelerRequestOTP dto)
        {
            var key = dto.Phone.HashWithNoSalt();
            if (appConfig.USE_FIXED_OTP)
            {
                var otp = AuthConstants.TEST_AUTHORIZE_OTP;
                if (!await cacheService.SetDataAsync(key, otp, CacheConstants.DEFAULT_VALID_MINUTE))
                    throw new SystemException(AppMessage.ERR_FUNCTION_DOWN);
                return true;
            }
            switch (dto.Channel)
            {
                case MessageChannel.VONAGE:
                    var request = new VerifyRequest()
                    {
                        Brand = config["Brandname"] ?? GlobalConstants.DEFAULT_BRAND_NAME,
                        Number = dto.Phone,
                        Country = "VN",
                        NextEventWait = CacheConstants.DEFAULT_VALID_MINUTE * 60,
                        CodeLength = (int)AuthConstants.OTP_LENGTH,
                        WorkflowId = VerifyRequest.Workflow.TTS
                    };
                    var response = await vonageClient.VerifyClient.VerifyRequestAsync(request);
                    if (response.IsSuccessVerifyResponse())
                    {
                        if (await cacheService.SetDataAsync(key, response.RequestId, CacheConstants.DEFAULT_VALID_MINUTE))
                            return true;
                        throw new SystemException(AppMessage.ERR_FUNCTION_DOWN);
                    }
                    throw new Exception(response.ErrorText);
            }
            throw new SystemException(AppMessage.ERR_FUNCTION_DOWN);
        }
        #endregion
        #region Traveler request authorize
        public async Task<AuthResult> TravelerRequestAuthorizeAsync(TravelerAuth dto)
        {
            var account = await uow.GetRepo<Account>()
                                   .GetAll(true)
                                   .FirstOrDefaultAsync(a => a.Phone == dto.Phone && a.Role == Role.TRAVELER);
            List<Claim> claims = [];
            if (account == null)
            {
                claims.Add(new Claim(ClaimConstants.PHONE, dto.Phone));
                return GenerateAuthorizeTokens(claims);
            }
            if (!account.IsActive) throw new ArgumentException(AppMessage.ERR_ACCOUNT_BLOCKED);
            if (account.DeviceToken != dto.DeviceToken)
            {
                account.DeviceToken = dto.DeviceToken;
                if (!await uow.SaveChangesAsync()) throw new DbUpdateException(AppMessage.ERR_DB_UPDATE); 
            }
            backgroundService.ScheduleRemoveDevice(account.Id,
                                                   timeService.Now.AddMinutes(AuthConstants.REFRESH_TOKEN_LIFETIME_MINUTE));
            claims = [
                new Claim(ClaimConstants.ROLE, account.Role.ToString()),
                new Claim(ClaimConstants.ID, account.Id.ToString()),
                new Claim(ClaimConstants.REG_DATE, $"{account.CreatedAt:G}")
            ];
            return GenerateAuthorizeTokens(claims);
        }
        #endregion
        #region Traveler register
        public async Task<TravelerCreateResult> TravelerRegisterAsync(TravelerCreate dto)
        {
            var phone = claimService.GetClaim(ClaimConstants.PHONE, string.Empty);
            if (string.IsNullOrEmpty(phone)) throw new ArgumentException(AppMessage.ERR_CLAIM_NOT_FOUND);
            var accountRepo = uow.GetRepo<Account>();
            if (await accountRepo.GetAll().AnyAsync(t => t.Phone == phone)) throw new ArgumentException(AppMessage.ERR_ACCOUNT_PHONE_USED);
            var now = timeService.Now;
            var account = dto.Adapt<Account>();
            account.Phone = phone;
            account.CreatedAt = now;
            account.PrestigePoint = appConfig.DEFAULT_PRESTIGE_POINT;
            await accountRepo.AddAsync(account);
            if (await uow.SaveChangesAsync())
            {
                List<Claim> claims = [
                    new Claim(ClaimConstants.ROLE, account.Role.ToString()),
                    new Claim(ClaimConstants.ID, account.Id.ToString()),
                    new Claim(ClaimConstants.REG_DATE, account.CreatedAt.ToString(CultureInfo.InvariantCulture))
                ];
                return new TravelerCreateResult
                {
                    Account = account,
                    AuthResult = GenerateAuthorizeTokens(claims, now)
                };
            }
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Traveler sign out
        public async Task<Account> TravelerSignOutAsync()
        {
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            var accountRepo = uow.GetRepo<Account>();
            var account = await accountRepo.FindAsync(accountId) ?? throw new KeyNotFoundException(AppMessage.ERR_ACCOUNT_NOT_FOUND);
            account.DeviceToken = null;
            if (await uow.SaveChangesAsync()) return account;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Traveler update
        public async Task<Account> TravelerUpdateAsync(TravelerUpdate dto)
        {
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            var account = await uow.GetRepo<Account>().FindAsync(accountId)
                          ?? throw new KeyNotFoundException(AppMessage.ERR_ACCOUNT_NOT_FOUND);
            dto.Adapt(account);
            if (await uow.SaveChangesAsync()) return account;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Staff request authorize
        public async Task<AuthResult> StaffRequestAuthorizeAsync(StaffAuth dto)
        {
            List<Claim> claims = [];
            if (dto.Email != config["Admin:Email"])
            {
                var requestUID = claimService.GetUniqueRequestId();
                var key = string.Format(CacheConstants.STAFF_INFO_FORMAT, requestUID);
                var account = await cacheService.GetDataAsync<Account>(key);
                account ??= await uow.GetRepo<Account>()
                                     .GetAll()
                                     .FirstOrDefaultAsync(a => a.Email == dto.Email) ??
                                     throw new KeyNotFoundException(AppMessage.ERR_ACCOUNT_NOT_FOUND);
                claims = [
                    new Claim(ClaimConstants.ID, account.Id.ToString()),
                    new Claim(ClaimConstants.ROLE, account.Role.ToString())
                ];
                if (account.Role == Role.PROVIDER)
                    claims.Add(new Claim(ClaimConstants.PROVIDER_ID, account.ProviderId.ToString()!));
            }
            else claims.Add(new Claim(ClaimConstants.ROLE, Role.ADMIN.ToString()));
            return GenerateAuthorizeTokens(claims);
        }
        #endregion
        #region Create staff
        public async Task<Account> CreateStaffAsync(StaffCreate dto)
        {
            var account = dto.Adapt<Account>();
            account.CreatedAt = timeService.Now;
            account.Role = dto.ProviderId.HasValue ? Role.PROVIDER : Role.STAFF;
            await uow.GetRepo<Account>().AddAsync(account);
            if (await uow.SaveChangesAsync()) return account;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
    }
}
