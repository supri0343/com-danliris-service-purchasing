using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addtableGarmentDeliveryOrderNonPO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GarmentDeliveryOrderNonPOs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    ArrivalDate = table.Column<DateTimeOffset>(nullable: false),
                    BillNo = table.Column<string>(maxLength: 50, nullable: true),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CustomsId = table.Column<long>(nullable: false),
                    DOCurrencyCode = table.Column<string>(nullable: true),
                    DOCurrencyId = table.Column<long>(nullable: true),
                    DOCurrencyRate = table.Column<double>(nullable: true),
                    DODate = table.Column<DateTimeOffset>(nullable: false),
                    DONo = table.Column<string>(maxLength: 255, nullable: true),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    IncomeTaxId = table.Column<int>(maxLength: 255, nullable: true),
                    IncomeTaxName = table.Column<string>(maxLength: 255, nullable: true),
                    IncomeTaxRate = table.Column<double>(nullable: true),
                    InternNo = table.Column<string>(nullable: true),
                    IsClosed = table.Column<bool>(nullable: false),
                    IsCorrection = table.Column<bool>(nullable: true),
                    IsCustoms = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsInvoice = table.Column<bool>(nullable: false),
                    IsPayIncomeTax = table.Column<bool>(nullable: true),
                    IsPayVAT = table.Column<bool>(nullable: true),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    PaymentBill = table.Column<string>(maxLength: 50, nullable: true),
                    PaymentMethod = table.Column<string>(nullable: true),
                    PaymentType = table.Column<string>(nullable: true),
                    Remark = table.Column<string>(nullable: true),
                    ShipmentNo = table.Column<string>(nullable: true),
                    ShipmentType = table.Column<string>(nullable: true),
                    SupplierCode = table.Column<string>(maxLength: 255, nullable: true),
                    SupplierId = table.Column<long>(maxLength: 255, nullable: false),
                    SupplierIsImport = table.Column<bool>(nullable: false),
                    SupplierName = table.Column<string>(maxLength: 1000, nullable: true),
                    TotalAmount = table.Column<double>(nullable: false),
                    UId = table.Column<string>(maxLength: 255, nullable: true),
                    UseIncomeTax = table.Column<bool>(nullable: true),
                    UseVat = table.Column<bool>(nullable: true),
                    VatId = table.Column<int>(maxLength: 255, nullable: true),
                    VatRate = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentDeliveryOrderNonPOs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GarmentDeliveryOrderNonPOItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CurrencyCode = table.Column<string>(nullable: true),
                    CurrencyId = table.Column<long>(nullable: false),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    GarmentDeliveryOrderNonPOId = table.Column<long>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    Price = table.Column<double>(nullable: false),
                    ProductRemark = table.Column<string>(nullable: true),
                    Quantity = table.Column<double>(nullable: false),
                    UId = table.Column<string>(nullable: true),
                    UomId = table.Column<long>(nullable: false),
                    UomUnit = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentDeliveryOrderNonPOItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarmentDeliveryOrderNonPOItems_GarmentDeliveryOrderNonPOs_GarmentDeliveryOrderNonPOId",
                        column: x => x.GarmentDeliveryOrderNonPOId,
                        principalTable: "GarmentDeliveryOrderNonPOs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarmentDeliveryOrderNonPOItems_GarmentDeliveryOrderNonPOId",
                table: "GarmentDeliveryOrderNonPOItems",
                column: "GarmentDeliveryOrderNonPOId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentDeliveryOrderNonPOs_DONo",
                table: "GarmentDeliveryOrderNonPOs",
                column: "DONo",
                unique: true,
                filter: "[IsDeleted]=(0) AND [CreatedUtc]>CONVERT([datetime2],'2020-02-01 00:00:00.0000000')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarmentDeliveryOrderNonPOItems");

            migrationBuilder.DropTable(
                name: "GarmentDeliveryOrderNonPOs");
        }
    }
}
