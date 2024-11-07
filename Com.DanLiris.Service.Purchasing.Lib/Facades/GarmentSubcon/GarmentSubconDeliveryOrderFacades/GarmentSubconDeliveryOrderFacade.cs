using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderNonPOViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconDeliveryOrderViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubconDeliveryOrderFacades
{
    public class GarmentSubconDeliveryOrderFacade : IGarmentSubconDeliveryOrderFacades
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentSubconDeliveryOrder> dbSet;
        private readonly DbSet<GarmentSubconDeliveryOrderItem> dbSetItem;
        private readonly DbSet<GarmentPurchaseRequestItem> dbSetPRItem;
        private readonly IMapper mapper;
        public GarmentSubconDeliveryOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentSubconDeliveryOrder>();
            dbSetItem = dbContext.Set<GarmentSubconDeliveryOrderItem>();
            dbSetPRItem = dbContext.Set<GarmentPurchaseRequestItem>();
            this.serviceProvider = serviceProvider;

            mapper = serviceProvider == null ? null : (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public Tuple<List<GarmentSubconDeliveryOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentSubconDeliveryOrder> Query = this.dbSet.AsNoTracking().Include(x => x.Items)
                .Select(x => new GarmentSubconDeliveryOrder
                {
                    Id = x.Id,
                    DONo = x.DONo,
                    DODate = x.DODate,
                    ArrivalDate = x.ArrivalDate,
                    ProductOwnerId = x.ProductOwnerId,
                    ProductOwnerCode = x.ProductOwnerCode,
                    ProductOwnerName = x.ProductOwnerName,
                    CreatedBy = x.CreatedBy,
                    LastModifiedUtc = x.LastModifiedUtc,
                    RONo = x.RONo,
                    BeacukaiNo = x.BeacukaiNo,
                    BeacukaiType = x.BeacukaiType,
                    BeacukaiDate = x.BeacukaiDate,
                    Items = x.Items.Select(y => new GarmentSubconDeliveryOrderItem
                    {
                        Id = y.Id,
                        POSerialNumber = y.POSerialNumber,
                        CurrencyCode = y.CurrencyCode,
                        UomId = y.UomId,
                        UomUnit = y.UomUnit,
                    }),
                });

            List<string> searchAttributes = new List<string>()
            {
                "DONo", "RONo"/*,"SupplierName"*/
            };

            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentSubconDeliveryOrder> pageable = new Pageable<GarmentSubconDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentSubconDeliveryOrder> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentSubconDeliveryOrder ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentSubconDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
                        if (item.PRItemId != 0)
                        {
                            var pr = dbSetPRItem.FirstOrDefault(x => x.Id == item.PRItemId);
                            pr.RemainingQuantity -= item.DOQuantity;

                            EntityExtension.FlagForUpdate(pr, user, USER_AGENT);
                        }
                        else if (item.EPOItemId != 0)
                        {
                            GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(item.EPOItemId));
                            externalPurchaseOrderItem.DOQuantity += item.DOQuantity;

                            EntityExtension.FlagForUpdate(externalPurchaseOrderItem, user, USER_AGENT);
                        }
                    }

                    this.dbSet.Add(m);

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

        public async Task<int> Update(int id, GarmentSubconDeliveryOrder newModel, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldModel = this.dbSet
                              .Include(m => m.Items)
                              .SingleOrDefault(m => m.Id == id && !m.IsDeleted);


                    //Delete Item or Update Item Old Data
                    foreach (var item in oldModel.Items)
                    {
                        var existItem = newModel.Items.FirstOrDefault(x => x.Id == item.Id);

                        if (existItem == null)
                        {
                            EntityExtension.FlagForDelete(item, user, USER_AGENT);

                            if (item.PRItemId != 0)
                            {
                                var pr = dbSetPRItem.FirstOrDefault(x => x.Id == item.PRItemId);
                                pr.RemainingQuantity += item.DOQuantity;

                                EntityExtension.FlagForUpdate(pr, user, USER_AGENT);
                            }
                            else if (item.EPOItemId != 0)
                            {
                                GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(item.EPOItemId));
                                externalPurchaseOrderItem.DOQuantity -= item.DOQuantity;

                                EntityExtension.FlagForUpdate(externalPurchaseOrderItem, user, USER_AGENT);
                            }
                        }
                        else
                        {
                            if (item.DOQuantity != existItem.DOQuantity)
                            {
                                var diff = item.DOQuantity - existItem.DOQuantity;
                                if (item.PRItemId != 0)
                                {
                                  
                                    var pr = dbSetPRItem.FirstOrDefault(x => x.Id == item.PRItemId);
                                    pr.RemainingQuantity += diff;

                                    if (pr.RemainingQuantity < 0)
                                    {
                                        throw new Exception("Jumlah DO tidak boleh melebihi sisa.");
                                    }
                                    EntityExtension.FlagForUpdate(pr, user, USER_AGENT);
                                }
                                else if (item.EPOItemId != 0)
                                {
                                    GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(item.EPOItemId));
                                    externalPurchaseOrderItem.DOQuantity -= diff;

                                    if (externalPurchaseOrderItem.DOQuantity < 0)
                                    {
                                        throw new Exception("Jumlah DO tidak boleh melebihi sisa.");
                                    }

                                    EntityExtension.FlagForUpdate(externalPurchaseOrderItem, user, USER_AGENT);
                                }

                                item.DOQuantity = existItem.DOQuantity;
                                EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                            }
                        }
                    }

                    //Add new Item
                    foreach (var item in newModel.Items)
                    {
                        if (item.Id == 0)
                        {
                            if (item.PRItemId != 0)
                            {
                                var pr = dbSetPRItem.FirstOrDefault(x => x.Id == item.PRItemId);
                                pr.RemainingQuantity -= item.DOQuantity;

                                EntityExtension.FlagForUpdate(pr, user, USER_AGENT);
                            }
                            else if (item.EPOItemId != 0)
                            {
                                GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(item.EPOItemId));
                                externalPurchaseOrderItem.DOQuantity += item.DOQuantity;

                                EntityExtension.FlagForUpdate(externalPurchaseOrderItem, user, USER_AGENT);
                            }

                            item.GarmentDOId = oldModel.Id;
                            EntityExtension.FlagForCreate(item, user, USER_AGENT);
                            EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                            dbSetItem.Add(item);

                        }
                    }

                    //Update Header Statement
                    if (oldModel.DONo != newModel.DONo)
                    {
                        oldModel.DONo = newModel.DONo;
                    }
                    if (oldModel.DODate != newModel.DODate)
                    {
                        oldModel.DODate = newModel.DODate;
                    }
                    if (oldModel.ArrivalDate != newModel.ArrivalDate)
                    {
                        oldModel.ArrivalDate = newModel.ArrivalDate;
                    }
                    if (oldModel.Remark != newModel.Remark)
                    {
                        oldModel.Remark = newModel.Remark;
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

        public async Task<int> Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                                .Include(m => m.Items)
                                .SingleOrDefault(m => m.Id == id && !m.IsDeleted);

                    EntityExtension.FlagForDelete(model, user, USER_AGENT);
                    foreach (var item in model.Items)
                    {
                        if (item.PRItemId != 0)
                        {
                            var pr = dbSetPRItem.FirstOrDefault(x => x.Id == item.PRItemId);
                            pr.RemainingQuantity += item.DOQuantity;

                            EntityExtension.FlagForUpdate(pr, user, USER_AGENT);
                        }
                        else if (item.EPOItemId != 0)
                        {
                            GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(item.EPOItemId));
                            externalPurchaseOrderItem.DOQuantity -= item.DOQuantity;

                            EntityExtension.FlagForUpdate(externalPurchaseOrderItem, user, USER_AGENT);
                        }


                        EntityExtension.FlagForDelete(item, user, USER_AGENT);
                    }
                    Deleted = await dbContext.SaveChangesAsync();

                    await dbContext.SaveChangesAsync();
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

        public IQueryable<GarmentSubconDeliveryOrder> DOForCustoms(string Keyword, string Filter, string currencycode = null)
        {
            IQueryable<GarmentSubconDeliveryOrder> Query = this.dbSet.Include(s => s.Items);

            List<string> searchAttributes = new List<string>()
            {
                "DONo"
            };

            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);
            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>("{}");

            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureOrder(Query, OrderDictionary).Include(m => m.Items)
                .Where(s => s.CustomsId == 0 && s.Items.Any(x => x.CurrencyCode == currencycode));

            return Query;
        }

        public ReadResponse<object> ReadForUnitReceiptNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            long filterSupplierId = FilterDictionary.ContainsKey("SupplierId") ? long.Parse(FilterDictionary["SupplierId"]) : 0;
            FilterDictionary.Remove("SupplierId");

            IQueryable<GarmentSubconDeliveryOrder> Query = dbSet
                .Where(m => m.DONo.Contains(Keyword ?? "") && (filterSupplierId == 0 ? true : m.ProductOwnerId == filterSupplierId) && m.CustomsId != 0 /*&& m.IsReceived == false*/)
                .Select(m => new GarmentSubconDeliveryOrder
                {
                    Id = m.Id,
                    DONo = m.DONo,
                    RONo = m.RONo,
                    ProductOwnerId = m.ProductOwnerId,
                    ProductOwnerName = m.ProductOwnerName,
                    ProductOwnerCode = m.ProductOwnerCode,
                    BeacukaiNo = m.BeacukaiNo,
                    BeacukaiType = m.BeacukaiType,
                    BeacukaiDate = m.BeacukaiDate,
                    LastModifiedUtc = m.LastModifiedUtc,
                    Article = m.Article,
                    Items = m.Items.Where(s => s.DOQuantity != (double)s.ReceiptQuantity).Select(i => new GarmentSubconDeliveryOrderItem
                    {
                        Id = i.Id,
                        POSerialNumber = i.POSerialNumber,
                        DOQuantity = (i.DOQuantity - (double)i.ReceiptQuantity),
                        ReceiptQuantity = i.ReceiptQuantity,
                        PricePerDealUnit = i.PricePerDealUnit,
                        ProductId = i.ProductId,
                        ProductCode = i.ProductCode,
                        ProductName = i.ProductName,
                        ProductRemark = i.ProductRemark,
                        UomId = i.UomId,
                        UomUnit = i.UomUnit,
                        RONoMaster = i.RONoMaster,
                        Article = i.Article,
                    }).ToList()
                });

            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentSubconDeliveryOrder> pageable = new Pageable<GarmentSubconDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentSubconDeliveryOrder> DataModel = pageable.Data.ToList();
            int Total = pageable.TotalCount;

            List<GarmentSubconDeliveryOrderViewModel> DataViewModel = mapper.Map<List<GarmentSubconDeliveryOrderViewModel>>(DataModel);

            List<dynamic> listData = new List<dynamic>();
            listData.AddRange(
                DataViewModel.Select(s => new
                {
                    s.Id,
                    s.doNo,
                    s.roNo,
                    s.article,
                    s.supplier,
                    s.LastModifiedUtc,
                    s.beacukaiDate,
                    s.beacukaiNo,
                    s.beacukaiType,
                    items = s.items.Select(i => new
                    {
                        i.Id,
                        i.POSerialNumber,
                        i.Product,
                        i.DOQuantity,
                        i.PricePerDealUnit,
                        i.Uom,
                        i.RONoMaster,
                        i.Article
                    }).ToList()
                }).ToList()
            );

            return new ReadResponse<object>(listData, Total, OrderDictionary);
        }

        public IQueryable<GarmentDeliveryOrderReportViewModel> GetReportQueryDO(string no, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query = (from a in dbContext.GarmentSubconDeliveryOrders
                         join i in dbContext.GarmentSubconDeliveryOrderItems on a.Id equals i.GarmentDOId

                         join o in dbContext.GarmentBeacukaiItems on a.Id equals o.GarmentDOId into beaitems
                         from oo in beaitems.DefaultIfEmpty()
                         join r in dbContext.GarmentBeacukais on oo.BeacukaiId equals r.Id into beas
                         from rr in beas.DefaultIfEmpty()

                         join n in dbContext.GarmentSubconUnitReceiptNoteItems on i.Id equals n.DOItemId into p
                         from URNItem in p.DefaultIfEmpty()
                         join k in dbContext.GarmentSubconUnitReceiptNotes on URNItem.URNId equals k.Id into l
                         from URN in l.DefaultIfEmpty()

                         join u in dbContext.GarmentSubconUnitDeliveryOrderItems on URNItem.Id equals u.URNItemId into uu
                         from UDOItem in uu.DefaultIfEmpty()
                         join ui in dbContext.GarmentSubconUnitDeliveryOrders on UDOItem.UnitDOId equals ui.Id into udi
                         from UDO in udi.DefaultIfEmpty()

                         join uei in dbContext.GarmentSubconUnitExpenditureNoteItems on UDOItem.Id equals uei.UnitDOItemId into uenitem
                         from UENItem in uenitem.DefaultIfEmpty()
                         join ue in dbContext.GarmentSubconUnitExpenditureNotes on UENItem.UENId equals ue.Id into uen
                         from UEN in uen.DefaultIfEmpty()
                         where
                          a.IsDeleted == false && i.IsDeleted == false && URN.IsDeleted == false && URNItem.IsDeleted == false && rr.IsDeleted == false
                              && oo.IsDeleted == false && UDOItem.IsDeleted == false && UDO.IsDeleted == false && UENItem.IsDeleted == false && UEN.IsDeleted == false
                              && a.DONo == (string.IsNullOrWhiteSpace(no) ? a.DONo : no)
                              && a.DODate.AddHours(offset).Date >= DateFrom.Date
                              && a.DODate.AddHours(offset).Date <= DateTo.Date
                              && (rr.CustomsType == "BC 40" || rr.CustomsType == "BC 27")

                         select new GarmentDeliveryOrderReportViewModel
                         {
                             no = a.DONo,
                             doDate = a.DODate == null ? new DateTime(1970, 1, 1) : a.DODate,
                             arrivalDate = a.ArrivalDate,
                             roNo = a.RONo,
                             poNo = i.POSerialNumber,
                             dOQuantity = i.DOQuantity,
                             doUom = i.UomUnit,
                             productCode = i.ProductCode,
                             productName = i.ProductName,

                             URNNo = URN == null ? "-" : URN.URNNo,
                             URNDate = URN == null ? new DateTime(1970, 1, 1) : URN.ReceiptDate,
                             urnQuantity = URNItem == null ? 0 : URNItem.SmallQuantity,
                             urnUom = URNItem == null ? "-" : URNItem.SmallUomUnit,

                             UDONo = UDO == null ? "-" : UDO.UnitDONo,
                             UDODate = UDO == null ? new DateTime(1970, 1, 1) : UDO.UnitDODate,
                             udoQuantity = UDOItem == null ? 0 : UDOItem.Quantity,
                             udoUom = UDOItem == null ? "-" : UDOItem.UomUnit,

                             UENNo = UEN == null ? "-" : UEN.UENNo,
                             UENDate = UEN == null ? new DateTime(1970, 1, 1) : UEN.ExpenditureDate,
                             uenQuantity = UENItem == null ? 0 : UENItem.Quantity,
                             uenUom = UENItem == null ? "-" : UENItem.UomUnit,

                             BeacukaiNo = rr != null ? rr.BeacukaiNo : "-",
                             BeacukaiDate = rr != null ? rr.BeacukaiDate : DateTimeOffset.MinValue,
                             BeacukaiType = rr != null ? rr.CustomsType : "-",

                         });

            //var QueryGroup = Query.GroupBy(x => x.)

            return Query; ;
        }

        public Tuple<List<GarmentDeliveryOrderReportViewModel>, int> GetReportDO(string no, DateTime? dateFrom, DateTime? dateTo, int page, int size,string Order, int offset)

        {
            var Query = GetReportQueryDO(no, dateFrom, dateTo, offset);


            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.doDate).ThenByDescending(b => b.UDODate);
            }


            Pageable<GarmentDeliveryOrderReportViewModel> pageable = new Pageable<GarmentDeliveryOrderReportViewModel>(Query, page - 1, size);
            List<GarmentDeliveryOrderReportViewModel> Data = pageable.Data.ToList<GarmentDeliveryOrderReportViewModel>();
            int TotalData = Query.Count();

            return Tuple.Create(Query.ToList(), TotalData);
        }

        public MemoryStream GenerateExcelDO(string no, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQueryDO(no, dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.doDate).ThenByDescending(b => b.UDODate);
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Tiba", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Dipesan", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "No BC", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tipe BC", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Input BC", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "Nomor BUM", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal BUM", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Qty BUM", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan2", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Unit DO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Unit DO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Qty Unit DO", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan3", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "Nomor BUK", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal BUK", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Qty BUK", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan4", DataType = typeof(String) });



            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", 0, "", "", "", "", 0, "", "", "", "", "", "", 0, "", "", "", "", 0, "", "", "", "", 0, "");
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string date = item.arrivalDate == null ? "-" : item.arrivalDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string supplierDoDate = item.doDate == new DateTime(1970, 1, 1) ? "-" : item.doDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string URNDate = item.URNDate == new DateTime(1970, 1, 1) ? "-" : item.URNDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string UDODate = item.UDODate == new DateTime(1970, 1, 1) ? "-" : item.UDODate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string BcDate = item.BeacukaiDate == DateTimeOffset.MinValue ? "-" : item.BeacukaiDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string UENDate = item.UENDate == new DateTime(1970, 1, 1) ? "-" : item.UENDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.no, supplierDoDate, date, item.roNo, item.poNo, item.productCode, item.productName, item.dOQuantity, item.doUom, item.BeacukaiNo, item.BeacukaiType, BcDate, item.URNNo, URNDate, item.urnQuantity, item.urnUom, item.UDONo, UDODate, item.udoQuantity, item.udoUom, item.UENNo, UENDate, item.uenQuantity, item.uenUom);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }
    }


}
