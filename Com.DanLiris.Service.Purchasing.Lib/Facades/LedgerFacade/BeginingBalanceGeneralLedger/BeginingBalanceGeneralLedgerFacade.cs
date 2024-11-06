using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.Ledger;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.LedgerFacade.BeginingBalanceGeneralLedger
{
    public class BeginingBalanceGeneralLedgerFacade : IBeginingBalanceGeneralLedgerFacade
    {
        private readonly PurchasingDbContext _dbContext;
        private readonly IdentityService _identityService;
        private const string UserAgent = "Facade";

        public BeginingBalanceGeneralLedgerFacade(PurchasingDbContext dbContext, IdentityService identityService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
        }

        public ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<BeginingBalanceGeneralLedgerModel> Query = _dbContext.BeginingBalanceGeneralLedgers;

            List<string> searchAttributes = new List<string>()
            {
                "COANo", "JournalType"
            };

            Query = QueryHelper<BeginingBalanceGeneralLedgerModel>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<BeginingBalanceGeneralLedgerModel>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            //Query = QueryHelper<GarmentLedgerModel>.ConfigureOrder(Query, OrderDictionary);

            Query = Query.Select(m => new BeginingBalanceGeneralLedgerModel
            {
                Id = m.Id,
                COANo = m.COANo,
                JournalType = m.JournalType,
                CreatedAgent = m.CreatedAgent,
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc,
                CreatedUtc = m.CreatedUtc,
                BeginingFinishingPrintingCredit = m.BeginingFinishingPrintingCredit,
                BeginingFinishingPrintingDebit = m.BeginingFinishingPrintingDebit,
                BeginingGarmentCredit = m.BeginingGarmentCredit,
                BeginingGarmentDebit = m.BeginingGarmentDebit,
                BeginingTextileCredit = m.BeginingTextileCredit,
                BeginingTextileDebit = m.BeginingTextileDebit
            });

            Pageable<BeginingBalanceGeneralLedgerModel> pageable = new Pageable<BeginingBalanceGeneralLedgerModel>(Query, Page - 1, Size);
            List<BeginingBalanceGeneralLedgerModel> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            List<object> ListData = new List<object>();
            ListData.AddRange(Data.Select(s => new
            {
                s.Id,
                s.COANo,
                s.JournalType,
                s.CreatedAgent,
                s.CreatedBy,
                s.LastModifiedUtc,
                s.CreatedUtc,
                s.BeginingFinishingPrintingCredit,
                s.BeginingFinishingPrintingDebit,
                s.BeginingGarmentCredit,
                s.BeginingGarmentDebit,
                s.BeginingTextileCredit,
                s.BeginingTextileDebit
            }).ToList());

            return new ReadResponse<object>(ListData, TotalData, OrderDictionary);
        }

        public async Task<int> UploadExcelAsync(List<BeginingBalanceGeneralLedgerModel> listData)
        {
            int created = 0;
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var data in listData)
                    {
                        data.FlagForCreate(_identityService.Username, UserAgent);
                        _dbContext.BeginingBalanceGeneralLedgers.Add(data);

                    }

                    created += await _dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return created;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }

        }
    }
}
