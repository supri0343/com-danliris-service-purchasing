using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.Ledger;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.LedgerFacade
{
    public class GarmentLedgerFacade : IGarmentLedgerFacade
    {
        private readonly PurchasingDbContext _dbContext;
        private readonly IdentityService _identityService;
        private const string UserAgent = "Facade";

        public GarmentLedgerFacade(PurchasingDbContext dbContext, IdentityService identityService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
        }

        public ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentGeneralLedgerModel> Query = _dbContext.GarmentGeneralLedgers;

            List<string> searchAttributes = new List<string>()
            {
                "COANo", "JournalType", "AccountNo", "ProofNo", "Remark"
            };

            Query = QueryHelper<GarmentGeneralLedgerModel>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentGeneralLedgerModel>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            //Query = QueryHelper<GarmentGeneralLedgerModel>.ConfigureOrder(Query, OrderDictionary);

            Query = Query.Select(m => new GarmentGeneralLedgerModel
            {
                Id = m.Id,
                COANo = m.COANo,
                JournalType = m.JournalType,
                AccountNo = m.AccountNo,
                ProofNo = m.ProofNo,
                Date = m.Date,
                Remark = m.Remark,
                Debit = m.Debit,
                Credit = m.Credit,
                CreatedAgent = m.CreatedAgent,
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc,
                CreatedUtc = m.CreatedUtc
            });

            Pageable<GarmentGeneralLedgerModel> pageable = new Pageable<GarmentGeneralLedgerModel>(Query, Page - 1, Size);
            List<GarmentGeneralLedgerModel> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            List<object> ListData = new List<object>();
            ListData.AddRange(Data.Select(s => new
            {
                s.Id,
                s.COANo,
                Month = s.Date.Value.Month,
                s.JournalType,
                s.AccountNo,
                s.ProofNo,
                s.Date,
                s.Remark,
                s.Debit,
                s.Credit,
                s.CreatedAgent,
                s.CreatedBy,
                s.LastModifiedUtc,
                s.CreatedUtc

            }).OrderByDescending(s => s.Date));

            return new ReadResponse<object>(ListData, TotalData, OrderDictionary);
        }

        public async Task<int> UploadExcelAsync(List<GarmentGeneralLedgerModel> listData)
        {
            int created = 0;
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var data in listData)
                    {
                        EntityExtension.FlagForCreate(data, _identityService.Username, UserAgent);
                        _dbContext.GarmentGeneralLedgers.Add(data);
                        
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
