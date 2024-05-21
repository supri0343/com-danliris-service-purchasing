using System;using Asp.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentReports
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/bc-for-aval")]
    [Authorize]
    public class BCForAvalController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IMapper mapper;
        private readonly IBCForAval _facade;
        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;

        public BCForAvalController(IBCForAval facade, IServiceProvider serviceProvider)
        {
            this._facade = facade;
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

        [HttpGet]
        public IActionResult Get([FromBody]string buk)
        {
            //int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
            //string accept = Request.Headers["Accept"];

            try
            {
                //identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
                //identityService.TimezoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());
                //identityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");

                var data2 = _facade.GetQuery(buk);
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
