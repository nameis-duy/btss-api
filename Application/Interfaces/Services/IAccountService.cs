using Application.DTOs.Generic;
using Application.DTOs.Staff_Admin;
using Application.DTOs.Traveler;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IAccountService : IGenericService<Account>
    {
        IQueryable<Account> GetAccounts();
        Task<AuthResult> RefreshAuthAsync(string refreshToken);
        Task<bool> TravelerRequestOTPAsync(TravelerRequestOTP dto);
        Task<AuthResult> TravelerRequestAuthorizeAsync(TravelerAuth dto);
        Task<TravelerCreateResult> TravelerRegisterAsync(TravelerCreate dto);
        Task<Account> TravelerSignOutAsync();
        Task<AuthResult> StaffRequestAuthorizeAsync(StaffAuth dto);
        Task<Account> CreateStaffAsync(StaffCreate dto);
        Task<Account> TravelerUpdateAsync(TravelerUpdate dto);
    }
}
