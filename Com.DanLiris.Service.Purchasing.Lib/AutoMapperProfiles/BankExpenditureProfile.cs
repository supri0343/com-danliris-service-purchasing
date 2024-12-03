﻿using Com.DanLiris.Service.Purchasing.Lib.Models.BankExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.BankExpenditureNote;

namespace Com.DanLiris.Service.Purchasing.Lib.AutoMapperProfiles
{
    public class BankExpenditureProfile : BaseAutoMapperProfile
    {
        public BankExpenditureProfile()
        {
            CreateMap<BankExpenditureNoteModel, BankExpenditureNoteViewModel>()
                .ForPath(d => d.Date , opt => opt.MapFrom(s => s.Date))
                /* Bank */
                .ForPath(d => d.Bank.Id, opt => opt.MapFrom(s => s.BankId))
                .ForPath(d => d.Bank.BankCode, opt => opt.MapFrom(s => s.BankCode))
                .ForPath(d => d.Bank.AccountCOA, opt => opt.MapFrom(s => s.BankAccountCOA))
                .ForPath(d => d.Bank.AccountCurrencyId, opt => opt.MapFrom(s => s.BankCurrencyId))
                .ForPath(d => d.Bank.AccountName, opt => opt.MapFrom(s => s.BankAccountName))
                .ForPath(d => d.Bank.AccountNumber, opt => opt.MapFrom(s => s.BankAccountNumber))
                .ForPath(d => d.Bank.BankName, opt => opt.MapFrom(s => s.BankName))
                .ForPath(d => d.Bank.Currency.Id, opt => opt.MapFrom(s => s.BankCurrencyId))
                .ForPath(d => d.Bank.Currency.Code, opt => opt.MapFrom(s => s.BankCurrencyCode))
                .ForPath(d => d.Bank.Currency.Rate, opt => opt.MapFrom(s => s.BankCurrencyRate))
                .ForPath(d => d.BankCashNo, opt => opt.MapFrom(s => s.BankCashNo))


                /* Supplier */
                .ForPath(d => d.Supplier._id, opt => opt.MapFrom(s => s.SupplierId))
                .ForPath(d => d.Supplier.code, opt => opt.MapFrom(s => s.SupplierCode))
                .ForPath(d => d.Supplier.import, opt => opt.MapFrom(s => s.SupplierImport))
                .ForPath(d => d.Supplier.name, opt => opt.MapFrom(s => s.SupplierName))

                 .ForPath(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ReverseMap();

            CreateMap<BankExpenditureNoteDetailModel, BankExpenditureNoteDetailViewModel>()
                  .ForPath(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ReverseMap();

            CreateMap<BankExpenditureNoteItemModel, BankExpenditureNoteItemViewModel>()
                .ForPath(d => d._id, opt => opt.MapFrom(s => s.Id))
                .ReverseMap();
        }
    }
}
