﻿using System;using Asp.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Asp.Versioning;
namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.Expedition
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/expedition/purchasing-document-expeditions-report")]
    [Authorize]
    public class PurchasingDocumentExpeditionReportController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly PurchasingDocumentExpeditionReportFacade purchasingDocumentExpeditionReportFacade;

        public PurchasingDocumentExpeditionReportController(PurchasingDocumentExpeditionReportFacade purchasingDocumentExpeditionReportFacade)
        {
            this.purchasingDocumentExpeditionReportFacade = purchasingDocumentExpeditionReportFacade;
        }

        [HttpGet]
        public ActionResult Get([Bind(Prefix = "unitPaymentOrders[]")] List<string> unitPaymentOrders)
        {
            List<PurchasingDocumentExpeditionReportViewModel> Data = this.purchasingDocumentExpeditionReportFacade.GetReport(unitPaymentOrders);

            return Ok(new
            {
                apiVersion = ApiVersion,
                data = Data,
                message = General.OK_MESSAGE,
                statusCode = General.OK_STATUS_CODE
            });
        }
    }
}