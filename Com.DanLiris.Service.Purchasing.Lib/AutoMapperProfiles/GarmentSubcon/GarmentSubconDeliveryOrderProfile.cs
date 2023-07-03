using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconDeliveryOrderViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class GarmentSubconDeliveryOrderProfile : Profile
    {
        public GarmentSubconDeliveryOrderProfile()
        {
            CreateMap<GarmentSubconDeliveryOrder, GarmentSubconDeliveryOrderViewModel>()
                .ForPath(d => d.supplier.Id, opt => opt.MapFrom(s => s.ProductOwnerId))
                .ForPath(d => d.supplier.Code, opt => opt.MapFrom(s => s.ProductOwnerCode))
                .ForPath(d => d.supplier.Name, opt => opt.MapFrom(s => s.ProductOwnerName))

                .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.doNo, opt => opt.MapFrom(s => s.DONo))
                .ForMember(d => d.doDate, opt => opt.MapFrom(s => s.DODate))
                .ForMember(d => d.arrivalDate, opt => opt.MapFrom(s => s.ArrivalDate))

                .ForPath(d => d.remark, opt => opt.MapFrom(s => s.Remark))
                .ForPath(d => d.roNo, opt => opt.MapFrom(s => s.RONo))

                .ForPath(d => d.costCalculationId, opt => opt.MapFrom(s => s.CostCalculationId))
                .ForPath(d => d.customsId, opt => opt.MapFrom(s => s.CustomsId))

                .ForPath(d => d.beacukaiNo, opt => opt.MapFrom(s => s.BeacukaiNo))
                .ForPath(d => d.beacukaiDate, opt => opt.MapFrom(s => s.BeacukaiDate))
                .ForPath(d => d.beacukaiType, opt => opt.MapFrom(s => s.BeacukaiType))

                .ReverseMap();

            CreateMap<GarmentSubconDeliveryOrderItem, GarmentSubconDeliveryOrderItemViewModel>()
                .ForPath(d => d.Product.Id, opt => opt.MapFrom(s => s.ProductId))
                .ForPath(d => d.Product.Code, opt => opt.MapFrom(s => s.ProductCode))
                .ForPath(d => d.Product.Name, opt => opt.MapFrom(s => s.ProductName))
                .ForPath(d => d.Product.Remark, opt => opt.MapFrom(s => s.ProductRemark))
                .ForPath(d => d.Uom.Id, opt => opt.MapFrom(s => s.UomId))
                .ForPath(d => d.Uom.Unit, opt => opt.MapFrom(s => s.UomUnit))
                .ReverseMap();
        }                  
    }
}
