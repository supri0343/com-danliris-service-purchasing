using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconCustomOut;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Com.DanLiris.Service.Purchasing.Lib.Services;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.GarmentSubconCustomOutFacades
{
    public class GarmentSubconCustomOutFacade : IGarmentSubconCustomOutFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly DbSet<GarmentSubconCustomOut> dbSet;
        private readonly DbSet<GarmentSubconCustomOutItem> dbSetItem;
        private readonly DbSet<GarmentSubconCustomOutDetail> dbSetDetail;
        private readonly IMapper mapper;
        protected readonly IHttpClientService _http;
        public GarmentSubconCustomOutFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentSubconCustomOut>();
            dbSetItem = dbContext.Set<GarmentSubconCustomOutItem>();
            dbSetDetail = dbContext.Set<GarmentSubconCustomOutDetail>();
            this.serviceProvider = serviceProvider;
            _http = serviceProvider.GetService<IHttpClientService>();
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
            mapper = serviceProvider == null ? null : (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public Tuple<List<GarmentSubconCustomOut>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentSubconCustomOut> Query = this.dbSet.AsNoTracking().Include(x => x.Items)
                .Select(x => new GarmentSubconCustomOut
                {
                    Id = x.Id,
                    BCNo = x.BCNo,
                    BCDate = x.BCDate,
                    BCType = x.BCType,
                    ProductOwnerId = x.ProductOwnerId,
                    ProductOwnerCode = x.ProductOwnerCode,
                    ProductOwnerName = x.ProductOwnerName,
                    CreatedBy = x.CreatedBy,
                    LastModifiedUtc = x.LastModifiedUtc,
                    Category = x.Category,
                    Items = x.Items.Select(y => new GarmentSubconCustomOutItem
                    {
                        Id = y.Id,
                        UENNo = y.UENNo,
                        LocalSalesNoteNo = y.LocalSalesNoteNo,
                        Quantity = y.Quantity,
                    }),
                    
                }) ;

            var a = Query.Where(x => x.Items.Any(s => s.LocalSalesNoteNo == ""));
            List<string> searchAttributes = new List<string>()
            {
                "BCNo","Category"/*, "Items.UENNo" , "Items.LocalSalesNoteNo"*/
            };

            Query = QueryHelper<GarmentSubconCustomOut>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentSubconCustomOut>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSubconCustomOut>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentSubconCustomOut> pageable = new Pageable<GarmentSubconCustomOut>(Query, Page - 1, Size);
            List<GarmentSubconCustomOut> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentSubconCustomOut ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .ThenInclude(x => x.Details)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentSubconCustomOut m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    List<long> idsToUpdate = new List<long>();
                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
                        if (m.Category == "JASA")
                        {
                            //Update IsUsed Local Sales Note
                            idsToUpdate.Add(item.LocalSalesNoteId.Value);
                        } 

                        if(m.Category == "SISA" && item.UENId != 0)
                        {
                            //Update IsReceived BUK
                            var uen = dbContext.GarmentSubconUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                            uen.IsReceived = true;

                            EntityExtension.FlagForUpdate(uen, user, USER_AGENT);
                        }

                        if (item.Details.Count() > 0)
                        {
                            foreach (var detail in item.Details)
                            {
                                EntityExtension.FlagForCreate(detail, user, USER_AGENT);
                            }
                        }

                    }

                    this.dbSet.Add(m);

                    if (idsToUpdate.Count > 0)
                    {
                        await PutIsUsedSubconLocalSalesNote(idsToUpdate, true);
                    }

                    Created = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }
            return Created;
        }

        public async Task<int> Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                                .Include(m => m.Items)
                                .ThenInclude(x => x.Details)
                                .SingleOrDefault(m => m.Id == id && !m.IsDeleted);

                    EntityExtension.FlagForDelete(model, user, USER_AGENT);

                    List<long> idsToUpdate = new List<long>();
                    foreach (var item in model.Items)
                    {
                        if (model.Category == "JASA")
                        {
                            //Update IsUsed Local Sales Note
                            idsToUpdate.Add(item.LocalSalesNoteId.Value);

                        }

                        if (model.Category == "SISA" && item.UENId != 0)
                        {
                            //Update IsReceived BUK
                            var uen = dbContext.GarmentSubconUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                            uen.IsReceived = false;

                            EntityExtension.FlagForUpdate(uen, user, USER_AGENT);
                        }

                        if(item.Details.Count() > 0)
                        {
                            foreach (var detail in item.Details)
                            {
                                EntityExtension.FlagForDelete(detail, user, USER_AGENT);
                            }
                        }

                        EntityExtension.FlagForDelete(item, user, USER_AGENT);


                    }

                    if (idsToUpdate.Count > 0)
                    {
                        await PutIsUsedSubconLocalSalesNote(idsToUpdate, false);
                    }

                    Deleted = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Deleted;
        }

        public async Task<int> Update(int id, GarmentSubconCustomOut newModel, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldModel = this.dbSet
                                .Include(m => m.Items)
                                .ThenInclude(x => x.Details)
                                .SingleOrDefault(m => m.Id == id && !m.IsDeleted);

                    List<long> idsToUpdateFalse = new List<long>();
                    List<long> idsToUpdateTrue = new List<long>();

                    //Delete Item or Update Item Old Data
                    foreach (var item in oldModel.Items)
                    {
                        var existItem = newModel.Items.FirstOrDefault(x => x.Id == item.Id);

                        if (existItem == null)
                        {
                            EntityExtension.FlagForDelete(item, user, USER_AGENT);

                            if (oldModel.Category == "JASA")
                            {
                                //Update IsUsed Local Sales Note
                                idsToUpdateFalse.Add(item.LocalSalesNoteId.Value);

                            }

                            if (oldModel.Category == "SISA" && item.UENId != 0)
                            {
                                //Update IsReceived BUK
                                var uen = dbContext.GarmentSubconUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                                uen.IsReceived = false;

                                EntityExtension.FlagForUpdate(uen, user, USER_AGENT);
                            }

                            if (item.Details.Count() > 0)
                            {
                                foreach (var detail in item.Details)
                                {
                                    EntityExtension.FlagForDelete(detail, user, USER_AGENT);
                                }
                            }
                        }
                    }

                    //Add new Item
                    foreach (var item in newModel.Items)
                    {
                        if (item.Id == 0)
                        {
                            if (oldModel.Category == "JASA")
                            {
                                //Update IsUsed Local Sales Note
                                idsToUpdateTrue.Add(item.LocalSalesNoteId.Value);

                            }

                            if (oldModel.Category == "SISA" && item.UENId != 0)
                            {
                                //Update IsReceived BUK
                                var uen = dbContext.GarmentSubconUnitExpenditureNotes.FirstOrDefault(x => x.Id == item.UENId);
                                uen.IsReceived = true;

                                EntityExtension.FlagForUpdate(uen, user, USER_AGENT);
                            }
                            item.CustomsId = oldModel.Id;
                            EntityExtension.FlagForCreate(item, user, USER_AGENT);
                            EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                            dbSetItem.Add(item);


                            if (item.Details.Count() > 0)
                            {
                                foreach (var detail in item.Details)
                                {
                                    EntityExtension.FlagForCreate(detail, user, USER_AGENT);
                                    EntityExtension.FlagForUpdate(detail, user, USER_AGENT);

                                    dbSetDetail.Add(detail);
                                }
                            }
                           

                        }
                    }
                    

                    if (idsToUpdateFalse.Count > 0)
                    {
                        await PutIsUsedSubconLocalSalesNote(idsToUpdateFalse, false);
                    }

                    if (idsToUpdateTrue.Count > 0)
                    {
                        await PutIsUsedSubconLocalSalesNote(idsToUpdateTrue, true);
                    }

                    //Update Header Statement
                    if (oldModel.BCType != newModel.BCType)
                    {
                        oldModel.BCType = newModel.BCType;
                    }

                    EntityExtension.FlagForUpdate(oldModel, user, USER_AGENT);
                    Updated = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        protected async Task<string> PutIsUsedSubconLocalSalesNote(List<long> ids, bool isUsed)
        {
            var subconLocalSalesNoteUri = APIEndpoint.PackingInventory + $"garment-shipping/receipt-subcon-local-sales-notes/update-used";
            var subconLocalSalesNoteResponse = await _http.PutAsync(subconLocalSalesNoteUri, identityService.Token, new StringContent(JsonConvert.SerializeObject(new { Ids = ids, IsUsed = isUsed }), Encoding.UTF8, "application/json"));

            return subconLocalSalesNoteResponse.EnsureSuccessStatusCode().ToString();
        }
    }
}
