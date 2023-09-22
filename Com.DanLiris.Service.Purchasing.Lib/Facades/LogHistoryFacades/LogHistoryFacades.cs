using Com.DanLiris.Service.Purchasing.Lib.Models.LogHistory;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.LogHistoryFacade
{
    public class LogHistoryFacades
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<LogHistory> dbSet;
        public LogHistoryFacades(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
            this.dbContext = dbContext;
            dbSet = dbContext.Set<LogHistory>();
        }

        public void Create(string division, string activity)
        {
            LogHistory model = new LogHistory
            {
                Division = division,
                Activity = activity,
                CreatedDate = DateTime.Now,
                CreatedBy = identityService.Username
            };

            dbSet.Add(model);
        }
    }
}
