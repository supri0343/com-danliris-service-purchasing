using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconCustomOut;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconCustomOutVM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles.GarmentSubcon
{
    public class GarmentSubconCustomOutProfile : Profile
    {
        public GarmentSubconCustomOutProfile()
        {
            CreateMap<GarmentSubconCustomOut, GarmentSubconCustomOutVM>()
                .ForPath(d => d.productOwner.Id, opt => opt.MapFrom(s => s.ProductOwnerId))
                .ForPath(d => d.productOwner.Code, opt => opt.MapFrom(s => s.ProductOwnerCode))
                .ForPath(d => d.productOwner.Name, opt => opt.MapFrom(s => s.ProductOwnerName))

                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.bcNo, opt => opt.MapFrom(s => s.BCNo))
                .ForMember(d => d.bcDate, opt => opt.MapFrom(s => s.BCDate))
                .ForMember(d => d.bcType, opt => opt.MapFrom(s => s.BCType))
                .ForMember(d => d.category, opt => opt.MapFrom(s => s.Category))
                .ReverseMap();

            CreateMap<GarmentSubconCustomOutItem, GarmentSubconCustomOutItemVM>()
                .ForPath(d => d.uom.Id, opt => opt.MapFrom(s => s.UomId))
                .ForPath(d => d.uom.Unit, opt => opt.MapFrom(s => s.UomUnit))

                .ForPath(d => d.packageUom.Id, opt => opt.MapFrom(s => s.PackageUomId))
                .ForPath(d => d.packageUom.Unit, opt => opt.MapFrom(s => s.PackageUomUnit))

                .ReverseMap();

            CreateMap<GarmentSubconCustomOutDetail, GarmentSubconCustomOutDetailVM>()
              .ForPath(d => d.uom.Id, opt => opt.MapFrom(s => s.UomId))
              .ForPath(d => d.uom.Unit, opt => opt.MapFrom(s => s.UomUnit))
              
              .ForPath(d => d.product.Id, opt => opt.MapFrom(s => s.ProductId))
              .ForPath(d => d.product.Code, opt => opt.MapFrom(s => s.ProductCode))
              .ForPath(d => d.product.Name, opt => opt.MapFrom(s => s.ProductName))

              .ReverseMap();

        }
    }
}
