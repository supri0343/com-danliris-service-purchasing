using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class changeDODetailIdtoDOItemIdReceiptSubcon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DODetailId",
                table: "GarmentSubconUnitExpenditureNoteItems",
                newName: "DOItemId");

            migrationBuilder.RenameColumn(
                name: "DODetailId",
                table: "GarmentSubconUnitDeliveryOrderItems",
                newName: "DOItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DOItemId",
                table: "GarmentSubconUnitExpenditureNoteItems",
                newName: "DODetailId");

            migrationBuilder.RenameColumn(
                name: "DOItemId",
                table: "GarmentSubconUnitDeliveryOrderItems",
                newName: "DODetailId");
        }
    }
}
