﻿using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentUnitReceiptNoteViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles.GarmentSubcon
{
    public class GarmentSubconUnitRecetipNoteProfile : Profile
    {
        public GarmentSubconUnitRecetipNoteProfile()
        {
            CreateMap<GarmentSubconUnitReceiptNote, GarmentSubconUnitReceiptNoteViewModel>()
                .ForPath(d => d.Unit.Id, opt => opt.MapFrom(s => s.UnitId))
                .ForPath(d => d.Unit.Code, opt => opt.MapFrom(s => s.UnitCode))
                .ForPath(d => d.Unit.Name, opt => opt.MapFrom(s => s.UnitName))

                .ForPath(d => d.Supplier.Id, opt => opt.MapFrom(s => s.ProductOwnerId))
                .ForPath(d => d.Supplier.Code, opt => opt.MapFrom(s => s.ProductOwnerCode))
                .ForPath(d => d.Supplier.Name, opt => opt.MapFrom(s => s.ProductOwnerName))

                .ReverseMap();

            CreateMap<GarmentSubconUnitReceiptNoteItem, GarmentSubconUnitReceiptNoteItemViewModel>()
                .ForPath(d => d.Product.Id, opt => opt.MapFrom(s => s.ProductId))
                .ForPath(d => d.Product.Code, opt => opt.MapFrom(s => s.ProductCode))
                .ForPath(d => d.Product.Name, opt => opt.MapFrom(s => s.ProductName))
                .ForPath(d => d.Product.Remark, opt => opt.MapFrom(s => s.ProductRemark))

                .ForPath(d => d.Uom.Id, opt => opt.MapFrom(s => s.UomId))
                .ForPath(d => d.Uom.Unit, opt => opt.MapFrom(s => s.UomUnit))

                .ForPath(d => d.SmallUom.Id, opt => opt.MapFrom(s => s.SmallUomId))
                .ForPath(d => d.SmallUom.Unit, opt => opt.MapFrom(s => s.SmallUomUnit))

                .ForPath(d => d.Storage._id, opt => opt.MapFrom(s => s.StorageId))
                .ForPath(d => d.Storage.code, opt => opt.MapFrom(s => s.StorageCode))
                .ForPath(d => d.Storage.name, opt => opt.MapFrom(s => s.StorageName))

                .ReverseMap();
        }
    }
}
