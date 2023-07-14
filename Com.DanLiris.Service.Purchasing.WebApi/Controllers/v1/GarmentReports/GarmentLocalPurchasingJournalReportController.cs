using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.PDFTemplates;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentReport
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/garment-purchasing/report/garment-finance-local-purchasing-journals")]
    [Authorize]
    public class GarmentLocalPurchasingJournalReportController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IGarmentLocalPurchasingJournalReportFacade garmentLocalPurchasingJournalReportFacade;

        public GarmentLocalPurchasingJournalReportController(IGarmentLocalPurchasingJournalReportFacade garmentLocalPurchasingJournalReportFacade)
        {
            this.garmentLocalPurchasingJournalReportFacade = garmentLocalPurchasingJournalReportFacade;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            try
            {
                int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
                var data = garmentLocalPurchasingJournalReportFacade.GetReportData(dateFrom, dateTo, offset);
           
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data,
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE

                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> GetXls([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            try
            {
                byte[] xlsInBytes;
                int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
                var xls = garmentLocalPurchasingJournalReportFacade.GenerateExcel(dateFrom, dateTo, offset);
                
                //string filename = "Laporan Jurnal Pembelian Lokal";
                //if (month != null) filename += " " + month.ToString();
                //if (year != null) filename += "-" + year.ToString();
                //filename += ".xlsx";

                string filename = String.Format("Laporan Jurnal Pembelian Lokal - {0}.xlsx", DateTime.UtcNow.ToString("ddMMyyyy"));

                xlsInBytes = xls.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }
    }
}