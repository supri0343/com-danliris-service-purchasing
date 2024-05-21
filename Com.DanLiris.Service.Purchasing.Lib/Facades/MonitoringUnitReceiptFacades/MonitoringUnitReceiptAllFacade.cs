using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.MonitoringUnitReceiptAllViewModel;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
 

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.MonitoringUnitReceiptFacades
{
	public class MonitoringUnitReceiptAllFacade : IMonitoringUnitReceiptAllFacade
	{
		private readonly PurchasingDbContext dbContext;
		public readonly IServiceProvider serviceProvider;
		private readonly DbSet<GarmentUnitReceiptNote> dbSet;
		public MonitoringUnitReceiptAllFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
		{
			this.dbSet = dbContext.Set<GarmentUnitReceiptNote>();
			this.serviceProvider = serviceProvider;
			this.dbContext = dbContext;
		 
		}
		public IEnumerable<MonitoringUnitReceiptAll> GetReportQuery(string no, string refNo, string roNo,string doNo, string unit,string supplier, DateTime? dateFrom, DateTime? dateTo,int offset)
		{
			DateTime d1 = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
			DateTime d2 = dateTo == null ? DateTime.Now : (DateTime)dateTo;
		
			List<MonitoringUnitReceiptAll> list = new List<MonitoringUnitReceiptAll>();
			var Data = (from a in dbContext.GarmentUnitReceiptNotes
						join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
						join c in dbContext.GarmentDeliveryOrders on a.DOId equals c.Id
						join d in dbContext.GarmentExternalPurchaseOrderItems on b.EPOItemId equals d.Id
						join e in dbContext.GarmentExternalPurchaseOrders on d.GarmentEPOId equals e.Id
						where a.IsDeleted == false  
						   && ((d1 != new DateTime(1970, 1, 1)) ? (a.CreatedUtc.Date >= d1 && a.CreatedUtc.Date <= d2) : true)
						   && ((supplier != null) ? (a.SupplierCode == supplier) : true)
						   && ((unit != null) ? (a.UnitCode == unit) : true)
						   && ((no != null) ? (a.URNNo == no) : true)
						   && ((doNo  != null) ? (a.DONo == doNo) : true)
						   && ((roNo  != null) ? (b.RONo == roNo) : true)
						   && ((refNo != null) ? (b.POSerialNumber == refNo ) : true)
						select  new {	id= a.Id, no=a.URNNo, dateBon= a.ReceiptDate, unit=a.UnitName, supplier= a.SupplierName, shipmentType =c.ShipmentType, doNo= a.DONo,poEksternalNo=e.EPONo,poRefPR=b.POSerialNumber,design=b.DesignColor,
										roNo = b.RONo,article=d.Article,productCode=b.ProductCode,productName=b.ProductName, qty= b.ReceiptQuantity,uom=b.UomUnit, price= b.PricePerDealUnit, remark= b.ProductRemark, user= a.CreatedBy, createdBy= e.CreatedBy, internNo=c.InternNo}
						)
						.Distinct()
						.ToList();

			var Query = (from data in Data
						 select new MonitoringUnitReceiptAll
						{ 
							no=data.no,
							dateBon=(data.dateBon.AddHours(offset)).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
							unit=data.unit,
							supplier=data.supplier,
                            shipmentType=data.shipmentType,
							doNo=data.doNo,
							poEksternalNo=data.poEksternalNo,
							poRefPR=data.poRefPR,
							roNo=data.roNo,
							article=data.article,
							productCode=data.productCode,
							productName=data.productName,
							qty=data.qty,
							uom=data.uom,
                            price=data.price,
							remark=data.remark,
							user=data.user,
							design=data.design,
                            createdBy=data.createdBy,
							internNote=data.internNo

						}).OrderByDescending(s => s.dateBon);
			int i = 1;
			foreach (var item in Query)
			{
				list.Add(
					   new MonitoringUnitReceiptAll
					   {
						   id=i,
						   no = item.no,
						   dateBon = item.dateBon,
						   unit = item.unit,
						   supplier = item.supplier,
                           shipmentType = item.shipmentType,
						   doNo = item.doNo,
						   poEksternalNo = item.poEksternalNo,
						   poRefPR = item.poRefPR,
						   roNo = item.roNo,
						   article = item.article,
						   productCode = item.productCode,
						   productName = item.productName,
						   qty = item.qty,
						   uom = item.uom,
                           price = item.price,
						   remark = item.remark,
						   user = item.user,
						   design = item.design,
                           createdBy = item.createdBy,
						   internNote = item.internNote
					   });
				i++;
			}

				return list.AsQueryable();
		}

		public Tuple<List<MonitoringUnitReceiptAll>, int> GetReport(string no, string refNo, string roNo, string doNo, string unit, string supplier, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
		{
			List<MonitoringUnitReceiptAll> Query = GetReportQuery( no,  refNo,  roNo,  doNo,  unit,  supplier, dateFrom, dateTo,offset).ToList();
			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
		
			Pageable<MonitoringUnitReceiptAll> pageable = new Pageable<MonitoringUnitReceiptAll>(Query, page - 1, size);
			List<MonitoringUnitReceiptAll> Data = pageable.Data.ToList<MonitoringUnitReceiptAll>();
			int TotalData = pageable.TotalCount;

			return Tuple.Create(Data, TotalData);
		}
		public MemoryStream GenerateExcel(string no, string refNo, string roNo, string doNo, string unit, string supplier, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
		{ 
			var Query = GetReportQuery(no, refNo, roNo, doNo, unit, supplier, dateFrom, dateTo, offset);
			
			DataTable result = new DataTable();
			result.Columns.Add(new DataColumn() { ColumnName = "NOMOR BON TERIMA UNIT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL BON TERIMA UNIT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "UNIT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "JENIS SUPPLIER", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "SURAT JALAN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO PO EKSTERNAL", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "RO REFERENSI PR", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO RO", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "KODE BARANG", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "JUMLAH", DataType = typeof(decimal) });
			result.Columns.Add(new DataColumn() { ColumnName = "SATUAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "HARGA SATUAN", DataType = typeof(decimal) });
			result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "DESAIN COLOR", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "USER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "STAFF PEMBELIAN", DataType = typeof(string) });
			result.Columns.Add(new DataColumn() { ColumnName = "NOTA INTERN", DataType = typeof(String) });

			List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

			if (Query.ToArray().Count() == 0)
			{
				result.Rows.Add("", "", "", "", "", "","","","","","","",0,"",0,"","","","",""); // to allow column name to be generated properly for empty data as template
			}
			else
			{
                int index = 0;
                foreach (MonitoringUnitReceiptAll data in Query)
				{
                    index++;
                    string jenissupp = data.shipmentType == "" ? "Local" : "Import";
                    result.Rows.Add(data.no, data.dateBon, data.unit,data.supplier,jenissupp,data.doNo,data.poEksternalNo,data.poRefPR,data.roNo,data.productCode,data.productName,data.qty,data.uom,data.price,data.remark,data.design,data.user,data.createdBy,data.internNote);

				}
			
			}

			return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "Report", mergeCells) }, true);
		}

		//------------------Menu baru history Delet-MDP BUM Facede----------------------------------------//
		public IEnumerable<MonitoringUnitReceiptAllDeleted> GetDeleteReportQuery(string bonType, DateTime? dateFrom, DateTime? dateTo)
		{
			var offset = 7;
			DateTime d1 = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
			DateTime d2 = dateTo == null ? DateTime.Now : (DateTime)dateTo;

			List<MonitoringUnitReceiptAllDeleted> list = new List<MonitoringUnitReceiptAllDeleted>();
			var Data = (from a in dbContext.GarmentUnitReceiptNotes.IgnoreQueryFilters()
						join b in dbContext.GarmentUnitReceiptNoteItems.IgnoreQueryFilters() on a.Id equals b.URNId
						where a.IsDeleted == true
						   //where a.IsDeleted == false
						   && ((d1 != new DateTime(1970, 1, 1)) ? (a.DeletedUtc.Date >= d1 && a.DeletedUtc.Date <= d2) : true)
						   
						select new
						{
							id = a.Id,
							no = a.URNNo,
							dateBon = a.ReceiptDate,
							unit = a.UnitName,
							supplier = a.SupplierName,
							doNo = a.DONo,
							urnType = a.URNType,
							deletedUtc= a.DeletedUtc,
							deletedBy = a.DeletedBy,
							suplayerCd = a.SupplierCode,
							uenNo=a.UENNo,
							drnNo=a.DRNo,

							roNo = b.RONo,
							POSrNo = b.POSerialNumber,
							PdName = b.ProductName,
							RQuantity = b.ReceiptQuantity,
							UmUnt = b.UomUnit

						}
						)
						.Distinct()
						.ToList();

			var Query = (from data in Data
                         select new MonitoringUnitReceiptAllDeleted
                         {
                             no = data.no,
                             dateBon = (data.dateBon.AddHours(offset)).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
                             unit = data.unit,
                             supplier = data.supplier,
                             doNo = data.doNo,
                             urnType = data.urnType,
                             deletedUtc = (data.deletedUtc.AddHours(offset)).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
                             deletedBy = data.deletedBy,
                             suplayerCd = data.suplayerCd,
                             uenNo = data.uenNo,
                             drnNo = data.drnNo,

							 roNo = data.roNo,
							 POSrNo = data.POSrNo,
							 PdName = data.PdName,
							 RQuantity = data.RQuantity,
							 UmUnt = data.UmUnt

						 }).OrderByDescending(s => s.dateBon);
			int i = 1;
			foreach (var item in Query)
			{
				list.Add(
					   new MonitoringUnitReceiptAllDeleted
					   {
						   id = i,
						   no = item.no,
						   dateBon = item.dateBon,
						   unit = item.unit,
						   supplier = item.supplier,
						   doNo = item.doNo,
						   urnType = item.urnType,
						   deletedUtc = item.deletedUtc,
						   deletedBy = item.deletedBy,
						   suplayerCd = item.suplayerCd,
						   uenNo = item.uenNo,
						   drnNo = item.drnNo,

						   roNo = item.roNo,
						   POSrNo = item.POSrNo,
						   PdName = item.PdName,
						   RQuantity = item.RQuantity,
						   UmUnt = item.UmUnt

					   });
				i++;
			}

			return list.AsQueryable();
		}

		public Tuple<List<MonitoringUnitReceiptAllDeleted>, int> GetDeleteReport(string bonType, DateTime? dateFrom, DateTime? dateTo)
		{
			List<MonitoringUnitReceiptAllDeleted> Query = GetDeleteReportQuery(bonType,  dateFrom,  dateTo).ToList();
			//Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);

			//Pageable<MonitoringUnitReceiptAll> pageable = new Pageable<MonitoringUnitReceiptAll>(Query, page - 1, size);
			//List<MonitoringUnitReceiptAll> Data = pageable.Data.ToList<MonitoringUnitReceiptAll>();
			//int TotalData = pageable.TotalCount;

			return Tuple.Create(Query, Query.Count());
		}

		//---Mencoba Excel XLS bro MDP---//
		public MemoryStream GenerateDeletedExcel(string bonType, DateTime? dateFrom, DateTime? dateTo)
		{
			// Memfilter Query sesuai dengan kriteria Anda (bonType, dateFrom, dateTo)
			var Query = GetDeleteReportQuery(bonType, dateFrom, dateTo);

			DataTable result = new DataTable();
			result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(int) });
			result.Columns.Add(new DataColumn() { ColumnName = "TGL DELETE", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "USER DELETE", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO BON TERIMA", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "TGL BON", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "JENIS BON", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "UNIT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "KODE SUPPLIER", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "SUPPLIER", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "SURAT JALAN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "BON KELUAR", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "DRNO", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO RO", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO PO", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "BARANG", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "JUMLAH", DataType = typeof(decimal) });
			result.Columns.Add(new DataColumn() { ColumnName = "SATUAN", DataType = typeof(String) });

			List<(string, Enum, Enum)> mergeCells = new List<(string, Enum, Enum)>() { };

			if (Query.ToArray().Count() == 0)
			{
				result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, ""); // to allow column name to be generated properly for empty data as template
			}
			else
			{
				int index = 0;
				foreach (MonitoringUnitReceiptAllDeleted data in Query)
				{
					index++;
					result.Rows.Add(index, data.deletedUtc, data.deletedBy, data.no, data.dateBon, data.urnType, data.unit, data.suplayerCd, data.supplier, data.doNo, data.uenNo, data.drnNo, data.roNo, data.POSrNo, data.PdName, data.RQuantity, data.UmUnt);
				}
			}

			return Excel.CreateExcel(new List<(DataTable, string, List<(string, Enum, Enum)>)>() { (result, "Report", mergeCells) }, true);
		}

		//-----------------------------------//
		//--------------------------------------------------------------------------------------//
	}
}
