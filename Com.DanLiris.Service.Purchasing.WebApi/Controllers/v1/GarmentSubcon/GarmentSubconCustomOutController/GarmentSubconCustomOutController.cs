using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconCustomOut;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconCustomOutVM;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentSubcon.GarmentSubconCustomOutController
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/garment-subcon-custom-outs")]
    [Authorize]
    public class GarmentSubconCustomOutController : Controller
    {
        private string ApiVersion = "1.0.0";
        public readonly IServiceProvider serviceProvider;
        private readonly IMapper mapper;
        private readonly IGarmentSubconCustomOutFacade facade;
        private readonly IdentityService identityService;
        public GarmentSubconCustomOutController(IServiceProvider serviceProvider, IMapper mapper, IGarmentSubconCustomOutFacade facade)
        {
            this.serviceProvider = serviceProvider;
            this.mapper = mapper;
            this.facade = facade;
            this.identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
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

                var Data = facade.Read(page, size, order, keyword, filter);

                var listData = Data.Item1.Select(x => new
                {
                    x.Id,
                    x.BCNo,
                    x.BCType,
                    x.BCDate,
                    ProductOwner = new { Id = x.ProductOwnerId, Code = x.ProductOwnerCode, Name = x.ProductOwnerCode },
                    x.CreatedBy,
                    x.LastModifiedUtc,
                    x.Category,
                    x.Remark,
                    items = x.Items.Select(i => new
                    {
                        i.UENNo,
                        i.LocalSalesNoteNo
                        
                    }),
                }).ToList();

                var info = new Dictionary<string, object>
                    {
                        { "count", listData.Count },
                        { "total", Data.Item2 },
                        { "order", Data.Item3 },
                        { "page", page },
                        { "size", size }
                    };

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(listData, info);
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
                var model = facade.ReadById(id);
                var viewModel = mapper.Map<GarmentSubconCustomOutVM>(model);
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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GarmentSubconCustomOutVM ViewModel)
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
                identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
                IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

                validateService.Validate(ViewModel);

                var model = mapper.Map<GarmentSubconCustomOut>(ViewModel);

                await facade.Create(model, identityService.Username);

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

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] GarmentSubconCustomOutVM ViewModel)
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
                identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
                IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

                validateService.Validate(ViewModel);

                var model = mapper.Map<GarmentSubconCustomOut>(ViewModel);

                await facade.Update(id, model, identityService.Username);

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
            identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            try
            {
                await facade.Delete(id, identityService.Username);
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

        [HttpGet("by-local-sales-note")]
        public async Task<IActionResult> GetByLocalSalesNote([FromBody]List<string>noteNo)
        {
            try
            {
                //identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                var Data = await facade.GetByLocalSalesNote(noteNo);

                var info = new Dictionary<string, object>
                    {
                        { "count", Data.Count },
                    };

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(Data, info);
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
