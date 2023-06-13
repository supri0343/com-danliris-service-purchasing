using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderNonPOModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderNonPOViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class GarmentDeliveryOrderNonPOProfile : Profile
    {
        public GarmentDeliveryOrderNonPOProfile()
        {
            CreateMap<GarmentDeliveryOrderNonPO, GarmentDeliveryOrderNonPOViewModel>()
                 .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.doNo, opt => opt.MapFrom(s => s.DONo))
                .ForMember(d => d.doDate, opt => opt.MapFrom(s => s.DODate))
                .ForMember(d => d.arrivalDate, opt => opt.MapFrom(s => s.ArrivalDate))

                /*Supplier*/
                .ForPath(d => d.supplier.Id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.supplier.Code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.supplier.Name, opt => opt.MapFrom(s => s.SupplierName))
                .ForPath(d => d.supplier.Import, opt => opt.MapFrom(s => s.SupplierIsImport))

                .ForPath(d => d.shipmentNo, opt => opt.MapFrom(s => s.ShipmentNo))
                .ForPath(d => d.shipmentType, opt => opt.MapFrom(s => s.ShipmentType))

                .ForPath(d => d.remark, opt => opt.MapFrom(s => s.Remark))
                .ForPath(d => d.isClosed, opt => opt.MapFrom(s => s.IsClosed))
                .ForPath(d => d.isCustoms, opt => opt.MapFrom(s => s.IsCustoms))
                .ForPath(d => d.isInvoice, opt => opt.MapFrom(s => s.IsInvoice))

                .ForPath(d => d.internNo, opt => opt.MapFrom(s => s.InternNo))
                .ForPath(d => d.billNo, opt => opt.MapFrom(s => s.BillNo))
                .ForPath(d => d.paymentBill, opt => opt.MapFrom(s => s.PaymentBill))
                 .ForPath(d => d.customsId, opt => opt.MapFrom(s => s.CustomsId))
                .ForPath(d => d.totalAmount, opt => opt.MapFrom(s => s.TotalAmount))

                .ForMember(d => d.isCorrection, opt => opt.MapFrom(s => s.IsCorrection))

                .ForPath(d => d.incomeTax.Id, opt => opt.MapFrom(s => s.IncomeTaxId))
                .ForPath(d => d.incomeTax.Name, opt => opt.MapFrom(s => s.IncomeTaxName))
                .ForPath(d => d.incomeTax.Rate, opt => opt.MapFrom(s => s.IncomeTaxRate))

                .ForPath(d => d.paymentMethod, opt => opt.MapFrom(s => s.PaymentMethod))
                .ForPath(d => d.paymentType, opt => opt.MapFrom(s => s.PaymentType))
                .ForPath(d => d.docurrency.Id, opt => opt.MapFrom(s => s.DOCurrencyId))
                .ForPath(d => d.docurrency.Code, opt => opt.MapFrom(s => s.DOCurrencyCode))
                .ForPath(d => d.docurrency.Rate, opt => opt.MapFrom(s => s.DOCurrencyRate))

                .ForPath(d => d.incomeTax.Id, opt => opt.MapFrom(s => s.IncomeTaxId))
                .ForPath(d => d.incomeTax.Name, opt => opt.MapFrom(s => s.IncomeTaxName))
                .ForPath(d => d.incomeTax.Rate, opt => opt.MapFrom(s => s.IncomeTaxRate))

                .ForPath(d => d.vat.Id, opt => opt.MapFrom(s => s.VatId))
                .ForPath(d => d.vat.Rate, opt => opt.MapFrom(s => s.VatRate))

                .ForPath(d => d.isPayIncomeTax, opt => opt.MapFrom(s => s.IsPayIncomeTax))
                .ForPath(d => d.isPayVAT, opt => opt.MapFrom(s => s.IsPayVAT))

                .ForMember(d => d.items, opt => opt.MapFrom(s => s.Items))

                .ReverseMap();

            CreateMap<GarmentDeliveryOrderNonPOItem, GarmentDeliveryOrderNonPOItemViewModel>()
               .ForMember(d => d._id, opt => opt.MapFrom(s => s.Id))

               .ForPath(d => d.currency.Id, opt => opt.MapFrom(s => s.CurrencyId))
               .ForPath(d => d.currency.Code, opt => opt.MapFrom(s => s.CurrencyCode))

               .ForPath(d => d.uom.Id, opt => opt.MapFrom(s => s.UomId))
               .ForPath(d => d.uom.Unit, opt => opt.MapFrom(s => s.UomUnit))

               .ReverseMap();

        }
    }
}
