using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addcoloumGReceiptSubconDOURNRONoMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RONoMaster",
                table: "GarmentSubconUnitReceiptNoteItems",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RONoMaster",
                table: "GarmentSubconDeliveryOrderItems",
                maxLength: 15,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RONoMaster",
                table: "GarmentSubconUnitReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "RONoMaster",
                table: "GarmentSubconDeliveryOrderItems");
        }
    }
}
