using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addtableSubconBUK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GarmentSubconUnitExpenditureNotes",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    ExpenditureDate = table.Column<DateTimeOffset>(nullable: false),
                    ExpenditureTo = table.Column<string>(maxLength: 1000, nullable: true),
                    ExpenditureType = table.Column<string>(maxLength: 255, nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsPreparing = table.Column<bool>(nullable: false),
                    IsReceived = table.Column<bool>(nullable: false),
                    IsTransfered = table.Column<bool>(nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    StorageCode = table.Column<string>(maxLength: 255, nullable: true),
                    StorageId = table.Column<long>(nullable: false),
                    StorageName = table.Column<string>(maxLength: 1000, nullable: true),
                    StorageRequestCode = table.Column<string>(maxLength: 255, nullable: true),
                    StorageRequestId = table.Column<long>(nullable: false),
                    StorageRequestName = table.Column<string>(maxLength: 1000, nullable: true),
                    UENNo = table.Column<string>(maxLength: 255, nullable: true),
                    UId = table.Column<string>(maxLength: 255, nullable: true),
                    UnitDOId = table.Column<long>(nullable: false),
                    UnitDONo = table.Column<string>(maxLength: 255, nullable: true),
                    UnitRequestCode = table.Column<string>(maxLength: 255, nullable: true),
                    UnitRequestId = table.Column<long>(nullable: false),
                    UnitRequestName = table.Column<string>(maxLength: 1000, nullable: true),
                    UnitSenderCode = table.Column<string>(maxLength: 255, nullable: true),
                    UnitSenderId = table.Column<long>(nullable: false),
                    UnitSenderName = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentSubconUnitExpenditureNotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GarmentSubconUnitExpenditureNoteItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false),
                    BasicPrice = table.Column<decimal>(type: "decimal(38, 4)", nullable: false),
                    BeacukaiDate = table.Column<DateTimeOffset>(nullable: false),
                    BeacukaiNo = table.Column<string>(nullable: true),
                    BeacukaiType = table.Column<string>(nullable: true),
                    Conversion = table.Column<decimal>(type: "decimal(38, 20)", nullable: false),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    DOCurrencyRate = table.Column<double>(nullable: true),
                    DODetailId = table.Column<long>(nullable: false),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    EPOItemId = table.Column<long>(nullable: false),
                    FabricType = table.Column<string>(maxLength: 255, nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ItemStatus = table.Column<string>(maxLength: 25, nullable: true),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    POItemId = table.Column<long>(nullable: false),
                    POSerialNumber = table.Column<string>(maxLength: 255, nullable: true),
                    PRItemId = table.Column<long>(nullable: false),
                    PricePerDealUnit = table.Column<double>(nullable: false),
                    ProductCode = table.Column<string>(maxLength: 255, nullable: true),
                    ProductId = table.Column<long>(nullable: false),
                    ProductName = table.Column<string>(maxLength: 255, nullable: true),
                    ProductOwnerCode = table.Column<string>(maxLength: 255, nullable: true),
                    ProductOwnerId = table.Column<long>(nullable: false),
                    ProductRemark = table.Column<string>(nullable: true),
                    Quantity = table.Column<double>(nullable: false),
                    RONo = table.Column<string>(maxLength: 255, nullable: true),
                    ReturQuantity = table.Column<double>(nullable: false),
                    UENId = table.Column<long>(nullable: false),
                    UId = table.Column<string>(maxLength: 255, nullable: true),
                    URNItemId = table.Column<long>(nullable: false),
                    UnitDOItemId = table.Column<long>(nullable: false),
                    UomId = table.Column<long>(nullable: false),
                    UomUnit = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentSubconUnitExpenditureNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarmentSubconUnitExpenditureNoteItems_GarmentSubconUnitExpenditureNotes_UENId",
                        column: x => x.UENId,
                        principalTable: "GarmentSubconUnitExpenditureNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarmentSubconUnitExpenditureNoteItems_UENId",
                table: "GarmentSubconUnitExpenditureNoteItems",
                column: "UENId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentSubconUnitExpenditureNotes_UENNo",
                table: "GarmentSubconUnitExpenditureNotes",
                column: "UENNo",
                unique: true,
                filter: "[IsDeleted]=(0) AND [CreatedUtc]>CONVERT([datetime2],'2020-02-01 00:00:00.0000000')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarmentSubconUnitExpenditureNoteItems");

            migrationBuilder.DropTable(
                name: "GarmentSubconUnitExpenditureNotes");
        }
    }
}
