using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addColoumRemainingQtyinPRPRitemIdinSJSubcon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PRItemId",
                table: "GarmentSubconDeliveryOrderItems",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "RemainingQuantity",
                table: "GarmentPurchaseRequestItems",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PRItemId",
                table: "GarmentSubconDeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "RemainingQuantity",
                table: "GarmentPurchaseRequestItems");
        }
    }
}
