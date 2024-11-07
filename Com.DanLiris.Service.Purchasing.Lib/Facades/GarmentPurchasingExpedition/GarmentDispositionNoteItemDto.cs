﻿namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingExpedition
{
    public class GarmentDispositionNoteItemDto
    {
        public GarmentDispositionNoteItemDto(int unitId, string unitCode, string unitName, int productId, string productName, double quantity, double price, string invoice)
        {
            UnitId = unitId;
            UnitCode = unitCode;
            UnitName = unitName;
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            Price = price;
            InvoiceNo = invoice;
        }

        public int UnitId { get; private set; }
        public string? UnitCode { get; private set; }
        public string? UnitName { get; private set; }
        public int ProductId { get; private set; }
        public string? ProductName { get; private set; }
        public double Quantity { get; private set; }
        public double Price { get; private set; }
        public string InvoiceNo { get; private set; }
    }
}