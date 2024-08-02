using AppAny.HotChocolate.FluentValidation;
using Application.DTOs.Generic;
using Application.DTOs.Staff_Admin;
using Application.DTOs.Traveler;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Infrastructure.Validators.Staff_Admin;
using Infrastructure.Validators.Traveler;

namespace API.GraphQL.Mutations
{
    public partial class Mutation
    {
        public async Task<AuthResult> RefreshAuthAsync([Service] IAccountService accountService, string refreshToken)
        {
            return await accountService.RefreshAuthAsync(refreshToken);
        }
        #region Traveler
        [Authorize]
        public async Task<TravelerCreateResult> TravelerRegisterAsync([Service] IAccountService accountService,
                                                                      [UseFluentValidation, UseValidator<TravelerCreateValidator>] TravelerCreate dto)
        {
            return await accountService.TravelerRegisterAsync(dto);
        }
        public async Task<bool> TravelerRequestOTPAsync([Service] IAccountService accountService,
                                                        [UseFluentValidation, UseValidator<TravelerRequestOTPValidator>] TravelerRequestOTP dto)
        {
            return await accountService.TravelerRequestOTPAsync(dto);
        }
        public async Task<AuthResult> TravelerRequestAuthorizeAsync([Service] IAccountService accountService,
                                                                    [UseFluentValidation, UseValidator<TravelerAuthValidator>] TravelerAuth dto)
        {
            return await accountService.TravelerRequestAuthorizeAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Account> TravelerSignOutAsync([Service] IAccountService accountService)
        {
            return await accountService.TravelerSignOutAsync();
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Account> TravelerUpdateAsync([Service] IAccountService accountService, [UseFluentValidation, UseValidator<TravelerUpdateValidator>] TravelerUpdate dto)
        {
            return await accountService.TravelerUpdateAsync(dto);
        }
        #endregion
        #region Staff/Admin
        public async Task<AuthResult> StaffRequestAuthorizeAsync([Service] IAccountService accountService,
                                                                 [UseFluentValidation, UseValidator<StaffAuthValidator>] StaffAuth dto)
        {
            return await accountService.StaffRequestAuthorizeAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public async Task<Account> CreateStaffAsync([Service] IAccountService accountService,
                                                    [UseFluentValidation, UseValidator<StaffCreateValidator>] StaffCreate dto)
        {
            return await accountService.CreateStaffAsync(dto);
        }
        #endregion
    }
}
