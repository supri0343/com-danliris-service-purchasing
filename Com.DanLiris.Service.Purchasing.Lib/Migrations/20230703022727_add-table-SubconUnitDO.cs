using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addtableSubconUnitDO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GarmentSubconUnitDeliveryOrders",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    Article = table.Column<string>(maxLength: 1000, nullable: true),
                    CorrectionId = table.Column<long>(nullable: false),
                    CorrectionNo = table.Column<string>(maxLength: 255, nullable: true),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    DOId = table.Column<long>(nullable: false),
                    DONo = table.Column<string>(maxLength: 255, nullable: true),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsUsed = table.Column<bool>(nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    OtherDescription = table.Column<string>(maxLength: 4000, nullable: true),
                    RONo = table.Column<string>(maxLength: 255, nullable: true),
                    StorageCode = table.Column<string>(maxLength: 255, nullable: true),
                    StorageId = table.Column<long>(nullable: false),
                    StorageName = table.Column<string>(maxLength: 1000, nullable: true),
                    StorageRequestCode = table.Column<string>(maxLength: 255, nullable: true),
                    StorageRequestId = table.Column<long>(nullable: false),
                    StorageRequestName = table.Column<string>(maxLength: 1000, nullable: true),
                    UENFromId = table.Column<long>(nullable: false),
                    UENFromNo = table.Column<string>(maxLength: 255, nullable: true),
                    UId = table.Column<string>(maxLength: 255, nullable: true),
                    UnitDODate = table.Column<DateTimeOffset>(nullable: false),
                    UnitDOFromId = table.Column<long>(nullable: false),
                    UnitDOFromNo = table.Column<string>(maxLength: 255, nullable: true),
                    UnitDONo = table.Column<string>(maxLength: 255, nullable: true),
                    UnitDOType = table.Column<string>(maxLength: 255, nullable: true),
                    UnitRequestCode = table.Column<string>(maxLength: 255, nullable: true),
                    UnitRequestId = table.Column<long>(nullable: false),
                    UnitRequestName = table.Column<string>(maxLength: 1000, nullable: true),
                    UnitSenderCode = table.Column<string>(maxLength: 255, nullable: true),
                    UnitSenderId = table.Column<long>(nullable: false),
                    UnitSenderName = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentSubconUnitDeliveryOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GarmentSubconUnitDeliveryOrderItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    BeacukaiDate = table.Column<DateTimeOffset>(nullable: false),
                    BeacukaiNo = table.Column<string>(nullable: true),
                    BeacukaiType = table.Column<string>(nullable: true),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    DOCurrencyRate = table.Column<double>(nullable: true),
                    DODetailId = table.Column<long>(nullable: false),
                    DefaultDOQuantity = table.Column<double>(nullable: false),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    DesignColor = table.Column<string>(maxLength: 1000, nullable: true),
                    EPOItemId = table.Column<long>(nullable: false),
                    FabricType = table.Column<string>(maxLength: 255, nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    POItemId = table.Column<long>(nullable: false),
                    POSerialNumber = table.Column<string>(maxLength: 255, nullable: true),
                    PRItemId = table.Column<long>(nullable: false),
                    PricePerDealUnit = table.Column<double>(nullable: false),
                    ProductCode = table.Column<string>(maxLength: 255, nullable: true),
                    ProductId = table.Column<long>(nullable: false),
                    ProductName = table.Column<string>(maxLength: 1000, nullable: true),
                    ProductRemark = table.Column<string>(maxLength: 1000, nullable: true),
                    Quantity = table.Column<double>(nullable: false),
                    RONo = table.Column<string>(maxLength: 255, nullable: true),
                    ReturQuantity = table.Column<double>(nullable: false),
                    ReturUomId = table.Column<long>(nullable: true),
                    ReturUomUnit = table.Column<string>(maxLength: 255, nullable: true),
                    UId = table.Column<string>(maxLength: 255, nullable: true),
                    URNId = table.Column<long>(nullable: false),
                    URNItemId = table.Column<long>(nullable: false),
                    URNNo = table.Column<string>(maxLength: 255, nullable: true),
                    UnitDOId = table.Column<long>(nullable: false),
                    UomId = table.Column<long>(nullable: false),
                    UomUnit = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentSubconUnitDeliveryOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarmentSubconUnitDeliveryOrderItems_GarmentSubconUnitDeliveryOrders_UnitDOId",
                        column: x => x.UnitDOId,
                        principalTable: "GarmentSubconUnitDeliveryOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarmentSubconUnitDeliveryOrderItems_UnitDOId",
                table: "GarmentSubconUnitDeliveryOrderItems",
                column: "UnitDOId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentSubconUnitDeliveryOrders_UnitDONo",
                table: "GarmentSubconUnitDeliveryOrders",
                column: "UnitDONo",
                unique: true,
                filter: "[IsDeleted]=(0) AND [CreatedUtc]>CONVERT([datetime2],'2020-02-01 00:00:00.0000000')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarmentSubconUnitDeliveryOrderItems");

            migrationBuilder.DropTable(
                name: "GarmentSubconUnitDeliveryOrders");
        }
    }
}
