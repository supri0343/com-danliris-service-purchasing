using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;using Asp.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentUnitReceiptNoteControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/garment-do-items")]
    [Authorize]
    public class GarmentDOItemController : Controller
    {
        private string ApiVersion = "1.0.0";
        public readonly IServiceProvider serviceProvider;
        private readonly IMapper mapper;
        private readonly IGarmentDOItemFacade facade;
        private readonly IdentityService identityService;

        public GarmentDOItemController(IServiceProvider serviceProvider, IGarmentDOItemFacade facade)
        {
            this.serviceProvider = serviceProvider;
            this.facade = facade;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

        [HttpGet("unit-delivery-order")]
        public IActionResult GetForUnitDO(string keyword = null, string filter = "{}")
        {
            try
            {
                var result = facade.ReadForUnitDO(keyword, filter);
                Dictionary<string, object> Result =
                       new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                       .Ok(result);
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

        [HttpGet("unit-delivery-order/more")]
        public IActionResult GetForUnitDOMore(string keyword = null, string filter = "{}")
        {
            try
            {
                var result = facade.ReadForUnitDOMore(keyword, filter);
                Dictionary<string, object> Result =
                       new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                       .Ok(result);
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

        [HttpGet("by-po")]
        public IActionResult GetDOItemsByPO(string productcode, string po, string unitcode)
        {
            try
            {
                var result = facade.GetByPO(productcode,po, unitcode);
                Dictionary<string, object> Result =
                       new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                       .Ok(result);
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

        [HttpGet("by-po/download")]
        public IActionResult GetXls(string productcode, string po, string unitcode)
        {
            try
            {
                byte[] xlsInBytes;

                int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
                var xls = facade.GenerateExcel(productcode, po, unitcode);

                string filename = "Laporan Inventory Racking";
                if (productcode != "" ) filename += " " + productcode;
                if (po != "") filename += "_" + po;
                if (unitcode != "") filename += "_" + unitcode;
                filename += ".xlsx";


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

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {

                var viewModel = facade.ReadById(id);
                if (viewModel == null)
                {
                    throw new Exception("Invalid Id");
                }

                Dictionary<string, object> Result = 
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                      .Ok(viewModel);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] DOItemsRackingViewModels ViewModel)
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
                identityService.TimezoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());

                IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

                //validateService.Validate(ViewModel);

                //var model = mapper.Map<GarmentUnitReceiptNote>(ViewModel);

                await facade.Update(id, ViewModel);

                return NoContent();
            }
            catch (ServiceValidationExeption e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE)
                    .Fail(e);
                return BadRequest(Result);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("stelling/{id}")]
        public IActionResult GetStelling(int id)
        {
            try
            {
                var indexAcceptPdf = Request.Headers["Accept"].ToList().IndexOf("application/pdf");

                int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
                identityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");
                var result = facade.GetStellingQuery(id, offset);

                if (indexAcceptPdf < 0)
                {
                    Dictionary<string, object> Result =
                        new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                        .Ok(result.Result);
                    return Ok(Result);
                }
                else
                {
                    identityService.TimezoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());

                    var stream = facade.GeneratePdf(result.Result);

                    var po = result.Result.Select(x => x.POSerialNumber).Take(1).ToList();

                    var a = po[0];

                    return new FileStreamResult(stream, "application/pdf")
                    {
                        FileDownloadName = $"Racking - {po[0]}.pdf"
                    };
                }
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
