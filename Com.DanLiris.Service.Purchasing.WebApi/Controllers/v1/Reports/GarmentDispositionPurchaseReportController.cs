using System;using Asp.Versioning;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDispositionPurchaseFacades;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDispositionPurchase;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.Reports
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/reports/garment-disposition-purchase")]
    [Authorize]
    public class GarmentDispositionPurchaseReportController : Controller
    {
        private readonly IGarmentDispositionPurchaseFacade _service;
        private readonly IdentityService _identityService;
        private const string ApiVersion = "1.0";

        public GarmentDispositionPurchaseReportController(IServiceProvider serviceProvider)
        {
            _service = serviceProvider.GetService<IGarmentDispositionPurchaseFacade>();
            _identityService = serviceProvider.GetService<IdentityService>();
        }

        private void VerifyUser()
        {
            _identityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            _identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            _identityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string username, [FromQuery] int supplierId, [FromQuery] DateTimeOffset? dateFrom, [FromQuery] DateTimeOffset? dateTo, [FromQuery] int page, [FromQuery] int size)
        {
            try
            {
                VerifyUser();
                _identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                var Data = _service.GetReport(supplierId, username, dateFrom, dateTo, size, page);

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(Data);
                return Ok(Result);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("list-user")]
        public async Task<IActionResult> GetUser([FromQuery] string keyword)
        {
            try
            {
                VerifyUser();
                _identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                var Data = await _service.GetListUsers(keyword);

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(Data);
                return Ok(Result);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("xlsx")]
        public IActionResult DownloadExcel([FromQuery] string username, [FromQuery] int supplierId, [FromQuery] string supplierName, [FromQuery] DateTimeOffset? dateFrom, [FromQuery] DateTimeOffset? dateTo)
        {

            try
            {
                VerifyUser();

                var Data =  _service.GetReport(supplierId, username, dateFrom, dateTo, 0, 0);


                var stream = GenerateExcel(Data.Data, _identityService.TimezoneOffset, username,supplierName,dateFrom,dateTo);

                var filename = "Laporan Disposisi Pembayaran.xlsx";

                var bytes = stream.ToArray();

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
            }
            catch (Exception e)
            {
                var result = new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message).Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, result);
            }
        }

        private MemoryStream GenerateExcel(List<DispositionPurchaseReportTableDto> data, int timezoneOffset, string username, string supplierName, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {

            var title = "LAPORAN DISPOSISI PEMBAYARAN";

            const int headerRow = 1;
            const int startingRow = 6;
            const int tableGap = 3;
            const int columnA = 1;
            const int columnB = 2;
            const int columnC = 3;
            const int columnD = 4;
            const int columnE = 5;

            int row = 1;
            int col = 1;
            int maxCol = 12;


            using (var package = new ExcelPackage())
            {
                //#region headerExcel
                //var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                //worksheet.Cells[row,col].Value = title;
                //worksheet.Cells[row,col,row,maxCol].Merge = true;
                //worksheet.Cells[row,col,row,maxCol].Style.Font.Size = 20;
                //worksheet.Cells[row, col, row, maxCol].Style.Font.Bold = true;
                //row++;

                //row++;
                //worksheet.Cells[row, col].Value = "filter";
                //worksheet.Cells[row, col, row, maxCol].Merge = true;
                //worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                //row++;

                //worksheet.Cells[row, col].Value = "Supplier : "+supplierName;
                //worksheet.Cells[row, col, row, maxCol].Merge = true;
                //worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                //row++;

                //worksheet.Cells[row, col].Value = "Staff Pembelian : " + username;
                //worksheet.Cells[row, col, row, maxCol].Merge = true;
                //worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                //row++;

                //worksheet.Cells[row, col].Value = "Tanggal Awal Disposisi : " + (dateFrom.HasValue? dateFrom.GetValueOrDefault().ToString("dd MMM yyyy"):"");
                //worksheet.Cells[row, col, row, maxCol].Merge = true;
                //worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                //row++;

                //worksheet.Cells[row, col].Value = "Tanggal Akhir Disposisi : " + (dateTo.HasValue? dateTo.GetValueOrDefault().ToString("dd MMM yyyy"):"");
                //worksheet.Cells[row, col, row, maxCol].Merge = true;
                //worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                //row++;
                //#endregion

                //#region headerTable
                //row++;
                //int rowSpan = 1;
                //int colSpan = 1;
                //worksheet.Cells[row, col].Value = "No.";
                //worksheet.Cells[row, col, row+rowSpan, col].Merge = true;
                //worksheet.Cells[row, col, row+rowSpan, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                //col++;

                //worksheet.Cells[row, col].Value = "Staff Pembelian";
                //worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                //col++;

                //worksheet.Cells[row, col].Value = "Disposisi";
                //worksheet.Cells[row, col, row, col+colSpan].Merge = true;
                //worksheet.Cells[row, col, row, col+colSpan].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row, col+colSpan].Style.Font.Bold = true;
                ////col+=2;
                //row++;

                //worksheet.Cells[row, col].Value = "Nomor";
                //worksheet.Cells[row, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col].Style.Font.Bold = true;
                //col++;

                //worksheet.Cells[row, col].Value = "Tanggal";
                //worksheet.Cells[row, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col].Style.Font.Bold = true;
                //col++;
                //row--;

                //worksheet.Cells[row, col].Value = "Supplier";
                //worksheet.Cells[row, col, row, col + colSpan].Merge = true;
                //worksheet.Cells[row, col, row, col + colSpan].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row, col + colSpan].Style.Font.Bold = true;
                ////col += 2;
                //row++;

                //worksheet.Cells[row, col].Value = "Kode";
                //worksheet.Cells[row, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col].Style.Font.Bold = true;
                //col++;

                //worksheet.Cells[row, col].Value = "Nama";
                //worksheet.Cells[row, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col].Style.Font.Bold = true;
                //col++;
                //row--;

                //worksheet.Cells[row, col].Value = "Kategori";
                //worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                //col++;

                //worksheet.Cells[row, col].Value = "Tipe Bayar";
                //worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                //col++;

                //worksheet.Cells[row, col].Value = "No Proforma/Invoice";
                //worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                //col++;

                //worksheet.Cells[row, col].Value = "Tanggal Jatuh Tempo";
                //worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                //col++;

                //worksheet.Cells[row, col].Value = "Mata Uang";
                //worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                //col++;

                //worksheet.Cells[row, col].Value = "Nominal";
                //worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                //worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                //col++;
                //row += 2;
                //#endregion
                #region DataBody
                col = 1;
                
                //worksheet.Cells[row, col].LoadFromDataTable(dataTable, false);
                #endregion
                var Qr = data.ToArray();
                var q = data.ToList();
                var index = 0;
                foreach (DispositionPurchaseReportTableDto a in q)
                {
                    DispositionPurchaseReportTableDto dup = Array.Find(Qr, o => o.DispositionNo == a.DispositionNo);
                    if (dup != null)
                    {
                        if (dup.Count == 0)
                        {
                            index++;
                            dup.Count = index;
                        }
                    }
                    a.Count = dup.Count;
                }
                //data = q.AsQueryable();
                var dataTable = ConvertListDataToTable(data);
                foreach (KeyValuePair<DataTable, String> item in new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(dataTable, "Sheet 1") })
                {
                    var worksheet = package.Workbook.Worksheets.Add(item.Value);

                    #region headerExcel
                    //var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                    worksheet.Cells[row, col].Value = title;
                    worksheet.Cells[row, col, row, maxCol].Merge = true;
                    worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 20;
                    worksheet.Cells[row, col, row, maxCol].Style.Font.Bold = true;
                    row++;

                    row++;
                    worksheet.Cells[row, col].Value = "filter";
                    worksheet.Cells[row, col, row, maxCol].Merge = true;
                    worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                    row++;

                    worksheet.Cells[row, col].Value = "Supplier : " + supplierName;
                    worksheet.Cells[row, col, row, maxCol].Merge = true;
                    worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                    row++;

                    worksheet.Cells[row, col].Value = "Staff Pembelian : " + username;
                    worksheet.Cells[row, col, row, maxCol].Merge = true;
                    worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                    row++;

                    worksheet.Cells[row, col].Value = "Tanggal Awal Disposisi : " + (dateFrom.HasValue ? dateFrom.GetValueOrDefault().ToString("dd MMM yyyy") : "");
                    worksheet.Cells[row, col, row, maxCol].Merge = true;
                    worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                    row++;

                    worksheet.Cells[row, col].Value = "Tanggal Akhir Disposisi : " + (dateTo.HasValue ? dateTo.GetValueOrDefault().ToString("dd MMM yyyy") : "");
                    worksheet.Cells[row, col, row, maxCol].Merge = true;
                    worksheet.Cells[row, col, row, maxCol].Style.Font.Size = 14;
                    row++;
                    #endregion

                    #region headerTable
                    row++;
                    int rowSpan = 1;
                    int colSpan = 1;
                    worksheet.Cells[row, col].Value = "No.";
                    worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                    col++;

                    worksheet.Cells[row, col].Value = "Staff Pembelian";
                    worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                    col++;

                    worksheet.Cells[row, col].Value = "Disposisi";
                    worksheet.Cells[row, col, row, col + colSpan].Merge = true;
                    worksheet.Cells[row, col, row, col + colSpan].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row, col + colSpan].Style.Font.Bold = true;
                    //col+=2;
                    row++;

                    worksheet.Cells[row, col].Value = "Nomor";
                    worksheet.Cells[row, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    col++;

                    worksheet.Cells[row, col].Value = "Tanggal";
                    worksheet.Cells[row, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    col++;
                    row--;

                    worksheet.Cells[row, col].Value = "Supplier";
                    worksheet.Cells[row, col, row, col + colSpan].Merge = true;
                    worksheet.Cells[row, col, row, col + colSpan].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row, col + colSpan].Style.Font.Bold = true;
                    //col += 2;
                    row++;

                    worksheet.Cells[row, col].Value = "Kode";
                    worksheet.Cells[row, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    col++;

                    worksheet.Cells[row, col].Value = "Nama";
                    worksheet.Cells[row, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    col++;
                    row--;

                    worksheet.Cells[row, col].Value = "Kategori";
                    worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                    col++;

                    worksheet.Cells[row, col].Value = "Tipe Bayar";
                    worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                    col++;

                    worksheet.Cells[row, col].Value = "No Proforma/Invoice";
                    worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                    col++;

                    worksheet.Cells[row, col].Value = "Tanggal Jatuh Tempo";
                    worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                    col++;

                    worksheet.Cells[row, col].Value = "Mata Uang";
                    worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                    col++;

                    worksheet.Cells[row, col].Value = "Nominal";
                    worksheet.Cells[row, col, row + rowSpan, col].Merge = true;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Size = 12;
                    worksheet.Cells[row, col, row + rowSpan, col].Style.Font.Bold = true;
                    col++;
                    row += 2;
                    #endregion

                    worksheet.Cells["A11"].LoadFromDataTable(item.Key, false, OfficeOpenXml.Table.TableStyles.None);


                    //sheet.Cells["C1:D1"].Merge = true;
                    //sheet.Cells["C1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    //sheet.Cells["E1:F1"].Merge = true;
                    //sheet.Cells["C1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    Dictionary<string, int> counts = new Dictionary<string, int>();
                    Dictionary<string, int> countsType = new Dictionary<string, int>();
                    var docNo = data.ToArray();
                    int value;
                    foreach (var a in data)
                    {
                        //FactBeacukaiViewModel dup = Array.Find(docNo, o => o.BCType == a.BCType && o.BCNo == a.BCNo);
                        //if (counts.TryGetValue(a.Invoice + a.ExpenditureGoodId, out value))
                        //{
                        //    counts[a.Invoice + a.ExpenditureGoodId]++;
                        //}
                        //else
                        //{
                        //    counts[a.Invoice + a.ExpenditureGoodId] = 1;
                        //}

                        //FactBeacukaiViewModel dup1 = Array.Find(docNo, o => o.BCType == a.BCType);
                        if (countsType.TryGetValue(a.DispositionNo, out value))
                        {
                            countsType[a.DispositionNo]++;
                        }
                        else
                        {
                            countsType[a.DispositionNo] = 1;
                        }
                    }

                    //index = 8;
                    //foreach (KeyValuePair<string, int> b in counts)
                    //{
                    //    sheet.Cells["A" + index + ":A" + (index + b.Value - 1)].Merge = true;
                    //    sheet.Cells["A" + index + ":A" + (index + b.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    //    sheet.Cells["C" + index + ":C" + (index + b.Value - 1)].Merge = true;
                    //    sheet.Cells["C" + index + ":C" + (index + b.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    //    sheet.Cells["D" + index + ":D" + (index + b.Value - 1)].Merge = true;
                    //    sheet.Cells["D" + index + ":D" + (index + b.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    //    sheet.Cells["E" + index + ":E" + (index + b.Value - 1)].Merge = true;
                    //    sheet.Cells["E" + index + ":E" + (index + b.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    //    sheet.Cells["F" + index + ":F" + (index + b.Value - 1)].Merge = true;
                    //    sheet.Cells["F" + index + ":F" + (index + b.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    //    index += b.Value;
                    //}

                    index = 11;
                    foreach (KeyValuePair<string, int> c in countsType)
                    {

                        worksheet.Cells["A" + index + ":A" + (index + c.Value - 1)].Merge = true;
                        worksheet.Cells["A" + index + ":A" + (index + c.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells["B" + index + ":B" + (index + c.Value - 1)].Merge = true;
                        worksheet.Cells["B" + index + ":B" + (index + c.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells["C" + index + ":C" + (index + c.Value - 1)].Merge = true;
                        worksheet.Cells["C" + index + ":C" + (index + c.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells["D" + index + ":D" + (index + c.Value - 1)].Merge = true;
                        worksheet.Cells["D" + index + ":D" + (index + c.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells["E" + index + ":E" + (index + c.Value - 1)].Merge = true;
                        worksheet.Cells["E" + index + ":E" + (index + c.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells["F" + index + ":F" + (index + c.Value - 1)].Merge = true;
                        worksheet.Cells["F" + index + ":F" + (index + c.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        index += c.Value;
                    }
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();


                }

                //worksheet.Cells[worksheet.Cells.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }

        private DataTable ConvertListDataToTable(List<DispositionPurchaseReportTableDto> data)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("No",typeof(string)));
            dataTable.Columns.Add(new DataColumn("Staff Pembelian", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Nomor Disposisi", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Tanggal Disposisi", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Kode Supplier", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Nama Supplier", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Kategori", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Tipe Bayar", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Nomor Invoice", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Tanggal Jatuh Tempo", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Mata Uang", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Nominal", typeof(string)));

            var dataWithIndex = data.Select((item, index) => new { Index = index, Item = item }).ToList();

            foreach (var item in dataWithIndex)
            {
                dataTable.Rows.Add(
                    //item.Index.ToString(),
                    item.Item.Count.ToString(),
                    item.Item.StaffName,
                    item.Item.DispositionNo,
                    item.Item.DispositionDate.ToString("dd/MM/yyyy"),
                    item.Item.SupplierCode,
                    item.Item.SupplierName,
                    item.Item.Category,
                    item.Item.PaymentType,
                    item.Item.InvoiceNo,
                    item.Item.DueDate.ToString("dd/MM/yyyy"),
                    item.Item.CurrencyCode,
                    item.Item.Nominal.ToString("N2")
                    );
            }

            return dataTable;
        }
    }
}
