using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Net;
using System.Threading;
using OfficeOpenXml;
using Com.DanLiris.Service.Purchasing.Lib.Models.Ledger;
using Com.DanLiris.Service.Purchasing.Lib.Facades.LedgerFacade;
using Asp.Versioning;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.Ledger
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/ledger/garment-ledger")]
    [Authorize]
    public class LedgerController : Controller
    {
        private readonly IdentityService identityService;
        private readonly IValidateService _validateService;
        private string ApiVersion = "1.0.0";
        private readonly ConverterChecker converterChecker = new ConverterChecker();
        private readonly IMapper mapper;
        private readonly IGarmentLedgerFacade facade;

        public LedgerController(IdentityService identityService, IValidateService validateService, IGarmentLedgerFacade facade, IMapper mapper)
        {
            this.identityService = identityService;
            this._validateService = validateService;
            this.mapper = mapper;
            this.facade = facade;
        }

        protected void VerifyUser()
        {
            identityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            identityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            try
            {
                VerifyUser();

                var Data = facade.Read(page, size, order, keyword, filter);

                //var viewModel = mapper.Map<List<GarmentInvoiceViewModel>>(Data.Data);

                //List<object> listData = new List<object>();
                //listData.AddRange(
                //    viewModel.AsQueryable().Select(s => s).ToList()
                //);

                var info = new Dictionary<string, object>
                    {
                        { "count", Data.TotalData },
                        { "total", Data.TotalData },
                        { "order", Data.Order },
                        { "page", page },
                        { "size", size }
                    };

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(Data.Data, info);
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

        [HttpPost("upload")]
        public async Task<IActionResult> Post(IFormFile file, CancellationToken cancellationToken)
        {
            try
            {
                VerifyUser();

                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (file.Length > 0 && fileExtension.Equals(".xlsx"))
                {
                    var downPaymentList = await MapExcel(file, cancellationToken);
                    //Tuple<bool, List<ReportDto>> Validated = _employeeService.UploadValidate(employeeList);
                    //if (Validated.Item1)
                    //{
                    var result = await facade.UploadExcelAsync(downPaymentList);

                    return Created("", result);
                    //}
                    //else
                    //{
                    //    var stream = GenerateExcelLogError(Validated.Item2);

                    //    var filename = "Error Log - Upload Karyawan.xlsx";
                    //    var bytes = stream.ToArray();
                    //return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                    //}
                }
                else
                {
                    throw new Exception("File not valid!");
                }
            }
            catch (ServiceValidationExeption ex)
            {

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE)
                    .Fail(ex.Message);
                return StatusCode(General.BAD_REQUEST_STATUS_CODE, Result);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }

        private async Task<List<GarmentGeneralLedgerModel>> MapExcel(IFormFile file, CancellationToken cancellationToken)
        {
            var result = new List<GarmentGeneralLedgerModel>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream, cancellationToken);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 4; row <= rowCount; row++)
                    {
                        var data = new GarmentGeneralLedgerModel()
                        {
                            COANo = converterChecker.GeneratePureString(worksheet.Cells[row, 1]),
                            JournalType = converterChecker.GeneratePureString(worksheet.Cells[row, 3]),
                            AccountNo = converterChecker.GeneratePureString(worksheet.Cells[row, 4]),
                            ProofNo = converterChecker.GeneratePureString(worksheet.Cells[row, 5]),
                            Date = converterChecker.GeneratePureDateTime(worksheet.Cells[row, 6]),
                            Remark = converterChecker.GeneratePureString(worksheet.Cells[row, 7]),
                            Debit = converterChecker.GeneratePureDouble(worksheet.Cells[row, 8]),
                            Credit = converterChecker.GeneratePureDouble(worksheet.Cells[row, 9]),
                        };

                        result.Add(data);
                    }
                }

            }
            return result;
        }

    }
}
