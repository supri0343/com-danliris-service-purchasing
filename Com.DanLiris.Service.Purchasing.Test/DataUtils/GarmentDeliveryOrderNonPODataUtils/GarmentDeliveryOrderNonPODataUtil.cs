using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderNonPOFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderNonPOModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderNonPODataUtils
{
    public class GarmentDeliveryOrderNonPODataUtil
    {
        private readonly GarmentDeliveryOrderNonPOFacades facade;

        public GarmentDeliveryOrderNonPODataUtil(GarmentDeliveryOrderNonPOFacades facade)
        {
            this.facade = facade;
        }

        public async Task<GarmentDeliveryOrderNonPO> GetNewData()
        {
            Random rnd = new Random();
            long nowTicks = DateTimeOffset.Now.Ticks;
            string nowTicksA = $"{nowTicks}a";
            string nowTicksB = $"{nowTicks}b";

            return new GarmentDeliveryOrderNonPO
            {
                DONo = $"{nowTicksA}",

                SupplierId = nowTicks,
                SupplierCode = $"BuyerCode{nowTicksA}",
                SupplierName = $"BuyerName{nowTicksA}",

                DODate = DateTimeOffset.Now,
                ArrivalDate = DateTimeOffset.Now,

                ShipmentType = $"ShipmentType{nowTicksA}",
                ShipmentNo = $"ShipmentNo{nowTicksA}",

                Remark = $"Remark{nowTicksA}",

                IsClosed = false,
                IsCustoms = false,
                IsInvoice = false,

                UseVat = false,
                UseIncomeTax = false,
                IncomeTaxId = 0,
                IncomeTaxName = "",
                IncomeTaxRate =0,

                IsCorrection = false,

                InternNo = "InternNO1234",
                PaymentBill = "BB181122003",
                BillNo = "BP181122142947000001",
                PaymentType = "",
                PaymentMethod = "",
                DOCurrencyId = 0,
                DOCurrencyCode = "",
                DOCurrencyRate = 0,

                TotalAmount = nowTicks,
                CustomsId = 1,

                VatId = 1,
                VatRate = 1,


                Items = new List<GarmentDeliveryOrderNonPOItem>
                {
                    new GarmentDeliveryOrderNonPOItem
                    {
                        PricePerDealUnit = 1,
                        Quantity = 1,
                        CurrencyId = 1,
                        CurrencyCode = "IDR",
                        UomId = 1,
                        UomUnit = "",
                        ProductRemark = ""
                    }
                }
            };
        }

        public async Task<GarmentDeliveryOrderNonPO> GetTestData()
        {
            var data = await GetNewData();
            await facade.Create(data, "Unit Test");
            return data;
        }
    }
}
