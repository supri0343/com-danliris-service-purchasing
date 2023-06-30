using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class AddtableURNSubconmodifiedDOSubcon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupplierName",
                table: "GarmentSubconDeliveryOrders",
                newName: "ProductOwnerName");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "GarmentSubconDeliveryOrders",
                newName: "ProductOwnerId");

            migrationBuilder.RenameColumn(
                name: "SupplierCode",
                table: "GarmentSubconDeliveryOrders",
                newName: "ProductOwnerCode");

            migrationBuilder.AddColumn<string>(
                name: "Article",
                table: "GarmentSubconDeliveryOrders",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BeacukaiDate",
                table: "GarmentSubconDeliveryOrders",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "BeacukaiNo",
                table: "GarmentSubconDeliveryOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeacukaiType",
                table: "GarmentSubconDeliveryOrders",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReceived",
                table: "GarmentSubconDeliveryOrders",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GarmentSubconUnitReceiptNotes",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    Article = table.Column<string>(nullable: true),
                    BeacukaiDate = table.Column<DateTimeOffset>(nullable: false),
                    BeacukaiNo = table.Column<string>(nullable: true),
                    BeacukaiType = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    DOCurrencyRate = table.Column<double>(nullable: true),
                    DOId = table.Column<long>(nullable: false),
                    DONo = table.Column<string>(maxLength: 255, nullable: true),
                    DRId = table.Column<string>(nullable: true),
                    DRNo = table.Column<string>(nullable: true),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedReason = table.Column<string>(nullable: true),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    IsCorrection = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsStorage = table.Column<bool>(nullable: false),
                    IsUnitDO = table.Column<bool>(nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    ProductOwnerCode = table.Column<string>(maxLength: 255, nullable: true),
                    ProductOwnerId = table.Column<long>(nullable: false),
                    ProductOwnerName = table.Column<string>(maxLength: 1000, nullable: true),
                    RONo = table.Column<string>(maxLength: 255, nullable: true),
                    ReceiptDate = table.Column<DateTimeOffset>(nullable: false),
                    Remark = table.Column<string>(nullable: true),
                    UId = table.Column<string>(maxLength: 255, nullable: true),
                    URNNo = table.Column<string>(maxLength: 255, nullable: true),
                    URNType = table.Column<string>(nullable: true),
                    UnitCode = table.Column<string>(maxLength: 255, nullable: true),
                    UnitId = table.Column<long>(nullable: false),
                    UnitName = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentSubconUnitReceiptNotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GarmentSubconUnitReceiptNoteItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    Area = table.Column<string>(nullable: true),
                    Box = table.Column<string>(nullable: true),
                    Colour = table.Column<string>(nullable: true),
                    Conversion = table.Column<decimal>(type: "decimal(38, 20)", nullable: false),
                    CorrectionConversion = table.Column<decimal>(type: "decimal(38, 20)", nullable: false),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    DOItemId = table.Column<long>(nullable: false),
                    DOQuantity = table.Column<double>(nullable: false),
                    DRItemId = table.Column<string>(nullable: true),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    DesignColor = table.Column<string>(maxLength: 1000, nullable: true),
                    IsCorrection = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    OrderQuantity = table.Column<decimal>(nullable: false),
                    POSerialNumber = table.Column<string>(maxLength: 1000, nullable: true),
                    PricePerDealUnit = table.Column<decimal>(type: "decimal(20, 4)", nullable: false),
                    ProductCode = table.Column<string>(maxLength: 255, nullable: true),
                    ProductId = table.Column<long>(nullable: false),
                    ProductName = table.Column<string>(maxLength: 1000, nullable: true),
                    ProductRemark = table.Column<string>(nullable: true),
                    Rack = table.Column<string>(nullable: true),
                    ReceiptCorrection = table.Column<decimal>(nullable: false),
                    ReceiptQuantity = table.Column<decimal>(type: "decimal(20, 4)", nullable: false),
                    RemainingQuantity = table.Column<decimal>(nullable: false),
                    SmallQuantity = table.Column<decimal>(nullable: false),
                    SmallUomId = table.Column<long>(nullable: false),
                    SmallUomUnit = table.Column<string>(maxLength: 1000, nullable: true),
                    StorageCode = table.Column<string>(maxLength: 255, nullable: true),
                    StorageId = table.Column<long>(nullable: false),
                    StorageName = table.Column<string>(maxLength: 1000, nullable: true),
                    UId = table.Column<string>(nullable: true),
                    URNId = table.Column<long>(nullable: false),
                    UomId = table.Column<long>(nullable: false),
                    UomUnit = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentSubconUnitReceiptNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarmentSubconUnitReceiptNoteItems_GarmentSubconUnitReceiptNotes_URNId",
                        column: x => x.URNId,
                        principalTable: "GarmentSubconUnitReceiptNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarmentSubconUnitReceiptNoteItems_URNId",
                table: "GarmentSubconUnitReceiptNoteItems",
                column: "URNId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentSubconUnitReceiptNotes_URNNo",
                table: "GarmentSubconUnitReceiptNotes",
                column: "URNNo",
                unique: true,
                filter: "[IsDeleted]=(0) AND [CreatedUtc]>CONVERT([datetime2],'2020-02-01 00:00:00.0000000')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarmentSubconUnitReceiptNoteItems");

            migrationBuilder.DropTable(
                name: "GarmentSubconUnitReceiptNotes");

            migrationBuilder.DropColumn(
                name: "Article",
                table: "GarmentSubconDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "BeacukaiDate",
                table: "GarmentSubconDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "BeacukaiNo",
                table: "GarmentSubconDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "BeacukaiType",
                table: "GarmentSubconDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "IsReceived",
                table: "GarmentSubconDeliveryOrders");

            migrationBuilder.RenameColumn(
                name: "ProductOwnerName",
                table: "GarmentSubconDeliveryOrders",
                newName: "SupplierName");

            migrationBuilder.RenameColumn(
                name: "ProductOwnerId",
                table: "GarmentSubconDeliveryOrders",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "ProductOwnerCode",
                table: "GarmentSubconDeliveryOrders",
                newName: "SupplierCode");
        }
    }
}
