using Application.DTOs.Generic;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Announcement;
using Domain.Enums.Others;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Infrastructure.Constants;
using Mapster;
using Microsoft.Extensions.Options;

namespace Infrastructure.Implements.Services
{
    public class AnnouncementService : GenericService<Announcement>, IAnnouncementService
    {
        private readonly FirebaseMessaging fcm;
        public AnnouncementService(IOptionsSnapshot<AppConfig> configSnapshot,
                                   IUnitOfWork uow,
                                   ITimeService timeService,
                                   IClaimService claimService,
                                   ICacheService cacheService,
                                   FirebaseApp firebase) : base(configSnapshot,
                                                                      uow,
                                                                      timeService,
                                                                      claimService,
                                                                      cacheService)
        {
            fcm = FirebaseMessaging.GetMessaging(firebase);
        }
        #region Get announcements
        public IQueryable<Announcement> GetAnnouncements()
        {
            var source = uow.GetRepo<Announcement>().GetAll();
            var role = claimService.GetClaim(ClaimConstants.ROLE, Role.TRAVELER);
            switch (role)
            {
                case Role.TRAVELER:
                    var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
                    var regDate = claimService.GetClaim(ClaimConstants.REG_DATE, timeService.Now);
                    regDate = DateTime.SpecifyKind(regDate, DateTimeKind.Utc);
                    return source.Where(a => a.AccountId == accountId || (!a.AccountId.HasValue && a.CreatedAt > regDate));
                case Role.PROVIDER:
                    var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                    return source.Where(a => a.ProviderId == providerId);
                case Role.STAFF:
                    return source.Where(a => a.Provider!= null && a.Provider.Account == null);
            }
            return source;
        }
        #endregion
        #region Push to device
        public async Task PushToDevicesAsync(Announcement announcement, params string?[] deviceTokens)
        {
            try
            {
                var notification = announcement.Adapt<Notification>();
                if (deviceTokens.Length == 0) return;
                if (deviceTokens.Length == 1)
                {
                    var message = new Message
                    {
                        Notification = notification,
                        Token = deviceTokens[0]
                    };
                    var singleRes = await fcm.SendAsync(message);
                    Console.WriteLine("Single push id"+ singleRes);
                    return;
                }
                var multiMessage = new MulticastMessage
                {
                    Notification = notification,
                    Tokens = deviceTokens
                };
                var batchRes = await fcm.SendMulticastAsync(multiMessage);
                Console.WriteLine("Batch push success: "+ batchRes.SuccessCount);
            }
            catch (Exception ex)
            {
                //log
                Console.WriteLine(ex.Message);
                return;
            }
        }
        #endregion
    }
}
