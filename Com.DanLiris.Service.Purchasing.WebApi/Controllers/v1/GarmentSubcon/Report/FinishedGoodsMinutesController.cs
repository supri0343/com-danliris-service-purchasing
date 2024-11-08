using Asp.Versioning;
using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.Report.FinishedGoodsMinutes.FinishedGoodsMinutesFacades;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentSubcon.Report
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/for-finished-good-minutes")]
    [Authorize]
    public class FinishedGoodsMinutesController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IMapper mapper;
        private readonly IFinishedGoodsMinutesFacade _facade;
        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;

        public FinishedGoodsMinutesController(IFinishedGoodsMinutesFacade facade, IServiceProvider serviceProvider)
        {
            this._facade = facade;
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromBody] List<string> roNO)
        {
            try
            {
                var data2 = await _facade.GetByRONo(roNO);
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data2
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
    }
}
