using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderNonPOModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderNonPOViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderNonPOViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentPurchaseRequestViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentDeliveryOrderControllers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentDeliveryOrderNonPOControllers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.GarmentDeliveryOrderNonPOControllerTests
{
    public class GarmentDeliveryOrderNonPOControllerTests
    {
        private GarmentDeliveryOrderNonPOViewModel ViewModel
        {
            get
            {
                return new GarmentDeliveryOrderNonPOViewModel
                {
                    supplier = new SupplierViewModel(),
                    docurrency = new CurrencyViewModel(),
                    incomeTax = new IncomeTaxViewModel(),
                    items = new List<GarmentDeliveryOrderNonPOItemViewModel>
                    {
                        new GarmentDeliveryOrderNonPOItemViewModel()
                        {
                            currency = new CurrencyViewModel(),
                            uom = new UomViewModel(),
                        }
                    }
                };
            }
        }

        private GarmentDeliveryOrderNonPO Model
        {
            get
            {
                return new GarmentDeliveryOrderNonPO { };
            }
        }

        private ServiceValidationExeption GetServiceValidationExeption()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            List<ValidationResult> validationResults = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(this.ViewModel, serviceProvider.Object, null);
            return new ServiceValidationExeption(validationContext, validationResults);
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            return serviceProvider;
        }

        private GarmentDeliveryOrderNonPOController GetController(Mock<IGarmentDeliveryOrderNonPOFacade> facadeM, Mock<IValidateService> validateM, Mock<IMapper> mapper)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            var servicePMock = GetServiceProvider();
            if (validateM != null)
            {
                servicePMock
                    .Setup(x => x.GetService(typeof(IValidateService)))
                    .Returns(validateM.Object);
            }

            GarmentDeliveryOrderNonPOController controller = new GarmentDeliveryOrderNonPOController(servicePMock.Object, mapper.Object, facadeM.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = user.Object
                    }
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "7";

            return controller;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderNonPOViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderNonPO>(It.IsAny<GarmentDeliveryOrderNonPOViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentDeliveryOrderNonPO>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = await controller.Post(this.ViewModel);
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Success_Delete()
        {
            //Setup
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderNonPOViewModel>())).Verifiable();

            var mapperMock = new Mock<IMapper>();

            var facadeMock = new Mock<IGarmentDeliveryOrderNonPOFacade>();
            facadeMock
                .Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(1);

            //Act
            var controller = GetController(facadeMock, validateMock, mapperMock);
            var response = await controller.Delete(1);

            //Assert
            Assert.Equal((int)HttpStatusCode.NoContent, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Fail_Delete()
        {
            //Setup
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderNonPOViewModel>())).Verifiable();

            var mapperMock = new Mock<IMapper>();

            var facadeMock = new Mock<IGarmentDeliveryOrderNonPOFacade>();
            facadeMock
                .Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<string>()))
                .Throws(new Exception());

            //Act
            var controller = GetController(facadeMock, validateMock, mapperMock);
            var response = await controller.Delete(1);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Validate_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderNonPOViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = await controller.Post(this.ViewModel);
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Error_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderNonPOViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderNonPO>(It.IsAny<GarmentDeliveryOrderNonPOViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentDeliveryOrderNonPO>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = new GarmentDeliveryOrderNonPOController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);

            var response = await controller.Post(this.ViewModel);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_All_Data_By_User()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentDeliveryOrderNonPO>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderNonPOViewModel>>(It.IsAny<List<GarmentDeliveryOrderNonPO>>()))
                .Returns(new List<GarmentDeliveryOrderNonPOViewModel> { ViewModel });

            GarmentDeliveryOrderNonPOController controller = GetController(mockFacade, null, mockMapper);
            var response = controller.GetByUser();
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_All_Data_By_User_With_Filter()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentDeliveryOrderNonPO>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderNonPOViewModel>>(It.IsAny<List<GarmentDeliveryOrderNonPO>>()))
                .Returns(new List<GarmentDeliveryOrderNonPOViewModel> { ViewModel });

            GarmentDeliveryOrderNonPOController controller = GetController(mockFacade, null, mockMapper);
            var response = controller.GetByUser(filter: "{ 'IsClosed': false }");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_All_Data()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentDeliveryOrderNonPO>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderNonPOViewModel>>(It.IsAny<List<GarmentDeliveryOrderNonPO>>()))
                .Returns(new List<GarmentDeliveryOrderNonPOViewModel> { ViewModel });

            GarmentDeliveryOrderNonPOController controller = new GarmentDeliveryOrderNonPOController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get();
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_All_Data_By_User()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentDeliveryOrderNonPO>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderNonPOViewModel>>(It.IsAny<List<GarmentDeliveryOrderNonPO>>()))
                .Returns(new List<GarmentDeliveryOrderNonPOViewModel> { ViewModel });

            GarmentDeliveryOrderNonPOController controller = new GarmentDeliveryOrderNonPOController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.GetByUser();
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }


        [Fact]
        public void Should_Success_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentDeliveryOrderNonPO());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderNonPOViewModel>(It.IsAny<GarmentDeliveryOrderNonPO>()))
                .Returns(ViewModel);

            GarmentDeliveryOrderNonPOController controller = new GarmentDeliveryOrderNonPOController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentDeliveryOrderNonPO());

            var mockMapper = new Mock<IMapper>();

            GarmentDeliveryOrderNonPOController controller = new GarmentDeliveryOrderNonPOController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Success_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderNonPOViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderNonPO>(It.IsAny<GarmentDeliveryOrderNonPOViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(),  It.IsAny<GarmentDeliveryOrderNonPO>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = await controller.Put(It.IsAny<int>(), this.ViewModel);
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Validate_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderNonPOViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = await controller.Put(It.IsAny<int>(), this.ViewModel);
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }
        //       

        [Fact]
        public async Task Should_Error_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderNonPOViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderNonPO>(It.IsAny<GarmentDeliveryOrderNonPOViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentDeliveryOrderNonPOFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentDeliveryOrderNonPO>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = new GarmentDeliveryOrderNonPOController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);

            var response = await controller.Put(It.IsAny<int>(), It.IsAny<GarmentDeliveryOrderNonPOViewModel>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

    }
}
