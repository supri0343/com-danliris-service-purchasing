using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;using Asp.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.LogHistoryControllers
{

    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/log-history")]
    [Authorize]
    public class LogHistoryControllers : Controller
    {
        private string ApiVersion = "1.0.0";
        public readonly IServiceProvider serviceProvider;
        private readonly ILogHistoryFacades facade;
        private readonly IdentityService identityService;
        public LogHistoryControllers(IServiceProvider serviceProvider, ILogHistoryFacades facade)
        {
            this.serviceProvider = serviceProvider;
            this.facade = facade;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

        [HttpGet]
        public async Task<IActionResult> Get(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                var result = await facade.GetReportQuery(dateFrom, dateTo);

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
