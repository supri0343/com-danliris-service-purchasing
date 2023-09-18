using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentUnitReceiptNoteViewModels;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentSubcon.GarmentSubconUnitReceiptNoteController
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/garment-subcon-unit-receipt-notes")]
    [Authorize]
    public class GarmentSubconUnitReceiptNoteController : Controller
    {
        private string ApiVersion = "1.0.0";
        public readonly IServiceProvider serviceProvider;
        private readonly IMapper mapper;
        private readonly IGarmentSubconUnitReceiptNoteFacade facade;
        private readonly IdentityService identityService;
        public GarmentSubconUnitReceiptNoteController(IServiceProvider serviceProvider, IMapper mapper, IGarmentSubconUnitReceiptNoteFacade facade)
        {
            this.serviceProvider = serviceProvider;
            this.mapper = mapper;
            this.facade = facade;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

        [HttpGet("by-user")]
        public IActionResult GetByUser(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                string filterUser = string.Concat("'CreatedBy':'", identityService.Username, "'");
                if (filter == null || !(filter.Trim().StartsWith("{") && filter.Trim().EndsWith("}")) || filter.Replace(" ", "").Equals("{}"))
                {
                    filter = string.Concat("{", filterUser, "}");
                }
                else
                {
                    filter = filter.Replace("}", string.Concat(", ", filterUser, "}"));
                }

                return Get(page, size, order, keyword, filter);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                var model = facade.Read(page, size, order, keyword, filter);

                var info = new Dictionary<string, object>
                    {
                        { "count", model.Data.Count },
                        { "total", model.TotalData },
                        { "order", model.Order },
                        { "page", page },
                        { "size", size }
                    };

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(model.Data, info);
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

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var indexAcceptPdf = Request.Headers["Accept"].ToList().IndexOf("application/pdf");

                var model = facade.ReadById(id);
                var viewModel = mapper.Map<GarmentSubconUnitReceiptNoteViewModel>(model);
                if (viewModel == null)
                {
                    throw new Exception("Invalid Id");
                }

                //if (indexAcceptPdf < 0)
                //{
                    Dictionary<string, object> Result =
                        new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                        .Ok(viewModel);
                    return Ok(Result);
                //}
                //else
                //{
                //    identityService.TimezoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());

                //    var stream = facade.GeneratePdf(viewModel);

                //    return new FileStreamResult(stream, "application/pdf")
                //    {
                //        FileDownloadName = $"{viewModel.URNNo}.pdf"
                //    };
                //}
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GarmentSubconUnitReceiptNoteViewModel viewModel)
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
                identityService.TimezoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());
                identityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");

                IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));
                validateService.Validate(viewModel);

                var Model = mapper.Map<GarmentSubconUnitReceiptNote>(viewModel);

                await facade.Create(Model);

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE)
                    .Ok();
                return Created(String.Concat(Request.Path, "/", 0), Result);
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

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put(int id, [FromBody] GarmentSubconUnitReceiptNoteViewModel ViewModel)
        //{
        //    try
        //    {
        //        identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
        //        identityService.TimezoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());

        //        IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

        //        validateService.Validate(ViewModel);

        //        var model = mapper.Map<GarmentSubconUnitReceiptNote>(ViewModel);

        //        await facade.Update(id, model);

        //        return NoContent();
        //    }
        //    catch (ServiceValidationExeption e)
        //    {
        //        Dictionary<string, object> Result =
        //            new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE)
        //            .Fail(e);
        //        return BadRequest(Result);
        //    }
        //    catch (Exception e)
        //    {
        //        Dictionary<string, object> Result =
        //            new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
        //            .Fail();
        //        return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
        //    }
        //}

        [HttpPut("deleted/{id}")]
        public async Task<IActionResult> DeleteData([FromRoute] int id, [FromBody] GarmentSubconUnitReceiptNoteViewModel ViewModel)
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
                identityService.TimezoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());
                identityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");

                await facade.Delete(id, ViewModel.DeletedReason);
                return NoContent();
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("unit-delivery-order")]
        public IActionResult GetForUnitDO(string keyword = null, string filter = "{}")
        {
            try
            {
                var result = facade.ReadForUnitDO(keyword, filter);
                //var viewModel = mapper.Map<GarmentSubconUnitReceiptNoteViewModel>(result);

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
    }
}
