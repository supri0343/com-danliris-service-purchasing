using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.PDFTemplates;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.DeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentInvoiceControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/garment-invoices")]
    [Authorize]
    public class GarmentInvoiceController : Controller
    {
        private string ApiVersion = "1.0.0";
        public readonly IServiceProvider serviceProvider;
        private readonly IMapper mapper;
        private readonly IGarmentInvoice facade;
		private readonly IGarmentDeliveryOrderFacade DOfacade;
        private readonly IGarmentInternNoteFacade InternNofacade;
        private readonly IdentityService identityService;
	 

		public GarmentInvoiceController(IServiceProvider serviceProvider, IMapper mapper, IGarmentInvoice facade, IGarmentDeliveryOrderFacade DOfacade, IGarmentInternNoteFacade InternNofacade)
        {
            this.serviceProvider = serviceProvider;
            this.mapper = mapper;
            this.facade = facade;
			this.DOfacade = DOfacade;
			this.InternNofacade = InternNofacade;
            this.identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

		[HttpGet("by-user")]
		public IActionResult GetByUser(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
		{
			try
			{
				identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
				identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");

				string filterUser = string.Concat("'CreatedBy':'", identityService.Username, "'");
				if (filter == null || !(filter.Trim().StartsWith("{") && filter.Trim().EndsWith("}")) || filter.Replace(" ", "").Equals("{}"))
				{
					filter = string.Concat("{", filterUser, "}");
				}
				else
				{
					filter = filter.Replace("}", string.Concat(", ", filterUser, "}"));
				}

				return GetMerge(page, size, order, keyword, filter);
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
                identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");

                var Data = facade.Read(page, size, order, keyword, filter);

                var viewModel = mapper.Map<List<GarmentInvoiceViewModel>>(Data.Item1);

                List<object> listData = new List<object>();
                listData.AddRange(
                    viewModel.AsQueryable().Select(s => s).ToList()
                );

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
        [HttpGet("merge")]
		public IActionResult GetMerge(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
		{
			try
			{
				identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
				identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");

				var Data = facade.ReadMerge(page, size, order, keyword, filter);

				//var viewModel = mapper.Map<List<GarmentInvoiceViewModel>>(Data.Item1);

				var viewModel = Data.Item1.ToList();

                HashSet<long> deliveryOrderIds = new HashSet<long>(viewModel.SelectMany(vm => vm.items.Select( s => s.deliveryOrderId)));
                List<GarmentDeliveryOrder> garmentDeliveryOrders = DOfacade.ReadForInternNote(deliveryOrderIds.ToList());

                foreach (var d in viewModel)
                {
                    foreach (var item in d.items)
                    {
                       
                            var deliveryOrder = garmentDeliveryOrders.SingleOrDefault(gdo => gdo.Id == item.deliveryOrderId);
                            if (deliveryOrder != null)
                            {
                                var deliveryOrderViewModel = mapper.Map<GarmentDeliveryOrderViewModel>(deliveryOrder);
                                item.deliveryOrder.items = deliveryOrderViewModel.items;
                            
                        }
                    }
                }

                List<object> listData = new List<object>();
				listData.AddRange(
					//viewModel.AsQueryable().Select(s => new { 
					//	s.Id,
					//	s.CreatedBy,
					//	s.LastModifiedUtc,
					//	s.invoiceNo,
					//	s.internNoteNo,
					//	s.invoiceDate,
					//	s.supplier,
					//	s.npn,
					//	s.nph,
					//	s.internNoteId,
					//	items =s.items.Select( i => new { 
					//		i.deliveryOrderId,
					//		i.deliveryOrderNo,
     //                       deliveryOrder = new
     //                       {
					//			doNo = i.deliveryOrder.doNo,
     //                           items = i.deliveryOrder.items == null ? null : i.deliveryOrder.items.Select(doi => new
     //                           {
     //                               fulfillments = doi.fulfillments == null ? null : doi.fulfillments.Select(dof => new
     //                               {
     //                                   dof.Id,
     //                                   dof.receiptQuantity
     //                               })
     //                           })
     //                       }

     //                   })
					
					
					//}).ToList()


                    viewModel
					.AsQueryable()
					.GroupBy(s => s.invoiceNo)  // Group by invoiceNo
					.Select(g => g.First())     // Ambil elemen pertama dari setiap grup
					.Select(s => new
					{
						s.Id,
						s.CreatedBy,
						s.LastModifiedUtc,
						s.invoiceNo,
						s.internNoteNo,
						s.invoiceDate,
						s.supplier,
						s.npn,
						s.nph,
						s.internNoteId,
						s.useVat,
						s.isPayTax,
						s.useIncomeTax,
						items = s.items.Select(i => new
						{
							i.deliveryOrderId,
							i.deliveryOrderNo,
							deliveryOrder = new
							{
								doNo = i.deliveryOrder.doNo,
								items = i.deliveryOrder.items == null ? null : i.deliveryOrder.items.Select(doi => new
								{
									fulfillments = doi.fulfillments == null ? null : doi.fulfillments.Select(dof => new
									{
										dof.Id,
										dof.receiptQuantity
									})
								})
							}
						})
					})
					.ToList()

				);

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
		[HttpGet("pdf/income-tax/{id}")]
		public IActionResult GetIncomePDF(int id)
		{
			try
			{
				identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
				var indexAcceptPdf = Request.Headers["Accept"].ToList().IndexOf("application/pdf");

				GarmentInvoice model = facade.ReadById(id);
				GarmentInvoiceViewModel viewModel = mapper.Map<GarmentInvoiceViewModel>(model);
				if (viewModel == null)
				{
					throw new Exception("Invalid Id");
				}
				if (indexAcceptPdf < 0)
				{
					return Ok(new
					{
						apiVersion = ApiVersion,
						statusCode = General.OK_STATUS_CODE,
						message = General.OK_MESSAGE,
						data = viewModel,
					});
				}
				else
				{
					int clientTimeZoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());

                    /* tambahan */
                    /* get gsupplier */
                    string supplierUri = "master/garment-suppliers";
                    var httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));
                    var response = httpClient.GetAsync($"{Lib.Helpers.APIEndpoint.Core}{supplierUri}/{model.SupplierId}").Result.Content.ReadAsStringAsync();
                    Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                    SupplierViewModel supplier = JsonConvert.DeserializeObject<SupplierViewModel>(result.GetValueOrDefault("data").ToString());
                    /* tambahan */

                    IncomeTaxPDFTemplate PdfTemplateLocal = new IncomeTaxPDFTemplate();
					MemoryStream stream = PdfTemplateLocal.GeneratePdfTemplate(viewModel,supplier,clientTimeZoneOffset, DOfacade);

					return new FileStreamResult(stream, "application/pdf")
					{
						FileDownloadName = $"{viewModel.nph}.pdf"
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
		[HttpGet("{id}")]
		public IActionResult Get(int id)
		{
			try
			{
				identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
				var model = facade.ReadById(id);
				var viewModel = mapper.Map<GarmentInvoiceViewModel>(model);
				if (viewModel == null)
				{
					throw new Exception("Invalid Id");
				}

				viewModel.internNoteNo = facade.GetInternNoteNo(viewModel.Id);
                foreach (var item in viewModel.items)
                {
                    GarmentDeliveryOrder deliveryOrder = DOfacade.ReadById((int)item.deliveryOrder.Id);
                    if (deliveryOrder != null)
                    {
                        GarmentDeliveryOrderViewModel deliveryOrderViewModel = mapper.Map<GarmentDeliveryOrderViewModel>(deliveryOrder);
                        item.deliveryOrder.items = deliveryOrderViewModel.items;
						item.deliveryOrder.useIncomeTax = deliveryOrderViewModel.useIncomeTax;
						item.deliveryOrder.useVat = deliveryOrderViewModel.useVat;
						item.deliveryOrder.supplier = deliveryOrderViewModel.supplier;
						item.deliveryOrder.docurrency = deliveryOrderViewModel.docurrency;
						item.deliveryOrder.isCorrection = deliveryOrderViewModel.isCorrection;
						if(item.deliveryOrder.incomeTax !=null)
						{
							item.deliveryOrder.incomeTax.Id = (int)deliveryOrder.IncomeTaxId;
							item.deliveryOrder.incomeTax.Name = deliveryOrder.IncomeTaxName;
							item.deliveryOrder.incomeTax.Rate = (double)deliveryOrder.IncomeTaxRate ;
                        }
                    }

					foreach (var detail in item.details)
					{ 
						detail.receiptQuantity = item.deliveryOrder.items.Select(i => i.fulfillments.Sum(f => f.receiptQuantity)).FirstOrDefault();
						detail.correctionQuantity = item.deliveryOrder.items.Select(i => i.fulfillments.Sum(f => f.quantityCorrection)).FirstOrDefault();
                    }
                    //foreach (var doitems in item.deliveryOrder.items)
                    //{ 
                    //	foreach (var detail in doitems.fulfillments)
                    //                   {
                    //                       foreach (var detailInvoice in item.details.Where( s => s.dODetailId == detail.Id))
                    //                       {
                    //                           detailInvoice.receiptQuantity = detail.receiptQuantity ;

                    //                       }
                    //                   }

                    //}
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
		[HttpGet("pdf/vat/{id}")]
		public IActionResult GetVatPDF(int id)
		{
			try
			{
				identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
				var indexAcceptPdf = Request.Headers["Accept"].ToList().IndexOf("application/pdf");

				GarmentInvoice model = facade.ReadById(id);
				GarmentInvoiceViewModel viewModel = mapper.Map<GarmentInvoiceViewModel>(model);
				if (viewModel == null)
				{
					throw new Exception("Invalid Id");
				}
				if (indexAcceptPdf < 0)
				{
					return Ok(new
					{
						apiVersion = ApiVersion,
						statusCode = General.OK_STATUS_CODE,
						message = General.OK_MESSAGE,
						data = viewModel,
					});
				}
				else
				{
					int clientTimeZoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());

                    /* tambahan */
                    /* get gsupplier */
                    string supplierUri = "master/garment-suppliers";
                    var httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));
                    var response = httpClient.GetAsync($"{Lib.Helpers.APIEndpoint.Core}{supplierUri}/{model.SupplierId}").Result.Content.ReadAsStringAsync();
                    Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                    SupplierViewModel supplier = JsonConvert.DeserializeObject<SupplierViewModel>(result.GetValueOrDefault("data").ToString());
                    /* tambahan */

                    VatPDFTemplate PdfTemplateLocal = new VatPDFTemplate();
					MemoryStream stream = PdfTemplateLocal.GeneratePdfTemplate(viewModel,supplier,clientTimeZoneOffset, DOfacade);

					return new FileStreamResult(stream, "application/pdf")
					{
						FileDownloadName = $"{viewModel.npn}.pdf"
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
		[HttpPost]
		public async Task<IActionResult> Post([FromBody]GarmentInvoiceViewModel ViewModel)
		{
			try
			{
				identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
				identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");

				IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

				validateService.Validate(ViewModel);

				var model = mapper.Map<GarmentInvoice>(ViewModel);

				await facade.CreateMerge(model, ViewModel, identityService.Username);

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
				return StatusCode(General.BAD_REQUEST_STATUS_CODE, Result);
				//return BadRequest(Result);
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
		public IActionResult Delete([FromRoute]int id)
		{
			identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
			identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");

			try
			{
				facade.Delete(id, identityService.Username);
				return NoContent();
			}
			catch (Exception)
			{
				return StatusCode(General.INTERNAL_ERROR_STATUS_CODE);
			}
		}

		[HttpDelete("merge/{id}")]
		public async Task<IActionResult> DeleteMerge([FromRoute] int id)
		{
			identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
			identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");

			try
			{
				await facade.DeleteMerge(id, identityService.Username);
				return NoContent();
			}
			catch (Exception)
			{
				return StatusCode(General.INTERNAL_ERROR_STATUS_CODE);
			}
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> Put(int id, [FromBody]GarmentInvoiceViewModel ViewModel)
		{
			try
			{
				identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
				identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");

				IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

				validateService.Validate(ViewModel);

				var model = mapper.Map<GarmentInvoice>(ViewModel);

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

		 
	}
}