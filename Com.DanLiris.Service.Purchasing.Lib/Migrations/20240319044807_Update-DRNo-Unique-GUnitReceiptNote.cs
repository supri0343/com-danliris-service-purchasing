using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class UpdateDRNoUniqueGUnitReceiptNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DRNo",
                table: "GarmentUnitReceiptNotes",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarmentUnitReceiptNotes_DRNo",
                table: "GarmentUnitReceiptNotes",
                column: "DRNo",
                unique: true,
                filter: "[IsDeleted]=(0) AND [CreatedUtc]>CONVERT([datetime2],'2019-10-04 00:00:00.0000000') AND [DRNo] !=''");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GarmentUnitReceiptNotes_DRNo",
                table: "GarmentUnitReceiptNotes");

            migrationBuilder.AlterColumn<string>(
                name: "DRNo",
                table: "GarmentUnitReceiptNotes",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
