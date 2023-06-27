using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addtableGarmentSubconDeliveryOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GarmentSubconDeliveryOrders",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    ArrivalDate = table.Column<DateTimeOffset>(nullable: false),
                    CostCalculationId = table.Column<long>(nullable: false),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CustomsId = table.Column<long>(nullable: false),
                    DODate = table.Column<DateTimeOffset>(nullable: false),
                    DONo = table.Column<string>(maxLength: 255, nullable: true),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    RONo = table.Column<string>(nullable: true),
                    Remark = table.Column<string>(nullable: true),
                    SupplierCode = table.Column<string>(maxLength: 255, nullable: true),
                    SupplierId = table.Column<long>(maxLength: 255, nullable: false),
                    SupplierName = table.Column<string>(maxLength: 1000, nullable: true),
                    UId = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentSubconDeliveryOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GarmentSubconDeliveryOrderItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    BudgetQuantity = table.Column<double>(nullable: false),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    CurrencyCode = table.Column<string>(nullable: true),
                    DOQuantity = table.Column<double>(nullable: false),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    GarmentDOId = table.Column<long>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    POSerialNumber = table.Column<string>(nullable: true),
                    PricePerDealUnit = table.Column<double>(nullable: false),
                    ProductCode = table.Column<string>(maxLength: 100, nullable: true),
                    ProductId = table.Column<long>(maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(maxLength: 1000, nullable: true),
                    ProductRemark = table.Column<string>(nullable: true),
                    UomId = table.Column<string>(maxLength: 100, nullable: true),
                    UomUnit = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentSubconDeliveryOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarmentSubconDeliveryOrderItems_GarmentSubconDeliveryOrders_GarmentDOId",
                        column: x => x.GarmentDOId,
                        principalTable: "GarmentSubconDeliveryOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarmentSubconDeliveryOrderItems_GarmentDOId",
                table: "GarmentSubconDeliveryOrderItems",
                column: "GarmentDOId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentSubconDeliveryOrders_DONo",
                table: "GarmentSubconDeliveryOrders",
                column: "DONo",
                unique: true,
                filter: "[IsDeleted]=(0) AND [CreatedUtc]>CONVERT([datetime2],'2020-02-01 00:00:00.0000000')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarmentSubconDeliveryOrderItems");

            migrationBuilder.DropTable(
                name: "GarmentSubconDeliveryOrders");
        }
    }
}
