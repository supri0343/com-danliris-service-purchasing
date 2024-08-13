using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class AddColoumGDOReceiptSubconEPO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EPOId",
                table: "GarmentSubconDeliveryOrderItems",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "EPOItemId",
                table: "GarmentSubconDeliveryOrderItems",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "EPONo",
                table: "GarmentSubconDeliveryOrderItems",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EPOId",
                table: "GarmentSubconDeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "EPOItemId",
                table: "GarmentSubconDeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "EPONo",
                table: "GarmentSubconDeliveryOrderItems");
        }
    }
}
