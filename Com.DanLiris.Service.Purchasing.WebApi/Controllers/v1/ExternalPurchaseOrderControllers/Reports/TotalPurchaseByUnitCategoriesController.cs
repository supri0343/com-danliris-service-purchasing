﻿using System;using Asp.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade.Reports;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Asp.Versioning;
namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.ExternalPurchaseOrderControllers.Reports
{
	[Produces("application/json")]
	[ApiVersion("1.0")]
	[Route("v{version:apiVersion}/purchase-orders/reports/units-categories")]
	[Authorize]
	public class TotalPurchaseByUnitCategoriesController : Controller
    {
		private string ApiVersion = "1.0.0";
		private readonly IMapper _mapper;
		private readonly TotalPurchaseFacade _facade;
		private readonly IdentityService identityService;
		public TotalPurchaseByUnitCategoriesController(IMapper mapper, TotalPurchaseFacade facade, IdentityService identityService)
		{
			_mapper = mapper;
			_facade = facade;
			this.identityService = identityService;
		}
		[HttpGet]
		public IActionResult GetReport(string division,string categoryId, string currencyCode,DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order = "{}")
		{
			int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
			string accept = Request.Headers["Accept"];

			//try
			//{

			var data = _facade.GetTotalPurchaseByUnitCategoriesReport(division,categoryId,currencyCode,dateFrom, dateTo, offset);

			return Ok(new
			{
				apiVersion = ApiVersion,
				data = data,
				message = General.OK_MESSAGE,
				statusCode = General.OK_STATUS_CODE
			});
			//}
			//catch (Exception e)
			//{
			//	Dictionary<string, object> Result =
			//		new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
			//		.Fail();
			//	return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
			//}
		}

		[HttpGet("download")]
		public IActionResult GetXls(string divisionId, string categoryId, string currencyCode, DateTime? dateFrom, DateTime? dateTo)
		{

			//try
			//{
			byte[] xlsInBytes;
			int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
			DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : Convert.ToDateTime(dateFrom);
			DateTime DateTo = dateTo == null ? DateTime.Now : Convert.ToDateTime(dateTo);

			var xls = _facade.GenerateExcelTotalPurchaseByUnitCategories(divisionId, categoryId, currencyCode, dateFrom, dateTo, offset);

			string filename = String.Format("Laporan Total Pembelian Per Unit Kategori - {0}.xlsx", DateTime.UtcNow.ToString("ddMMyyyy"));

			xlsInBytes = xls.ToArray();
			var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
			return file;

			//}
			//catch (Exception e)
			//{
			//	Dictionary<string, object> Result =
			//		new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
			//		.Fail();
			//	return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
			//}
		}
	}
}