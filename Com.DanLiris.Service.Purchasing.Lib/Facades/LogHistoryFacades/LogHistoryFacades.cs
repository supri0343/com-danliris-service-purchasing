using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.LogHistory;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.LogHistoryViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.LogHistoryFacade
{
    public class LogHistoryFacades : ILogHistoryFacades
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

        public async Task<List<LogHistoryViewModel>> GetReportQuery(DateTime dateFrom, DateTime dateTo)
        {
            var query = await dbSet.Where(x => x.CreatedDate.AddHours(7).Date >= dateFrom.Date && x.CreatedDate.AddHours(7).Date <= dateTo.Date)
                .Select(x => new LogHistoryViewModel
                {
                    Activity = x.Activity,
                    Division = x.Division,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate.AddHours(7)
                }).ToListAsync();

            return query;
        }
    }
}
