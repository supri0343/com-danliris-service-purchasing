using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addcoloumsIsSubconInvoiceGDeliveryOrderWithandNonPO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSubconInvoice",
                table: "GarmentDeliveryOrders",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubconInvoice",
                table: "GarmentDeliveryOrderNonPOs",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSubconInvoice",
                table: "GarmentDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "IsSubconInvoice",
                table: "GarmentDeliveryOrderNonPOs");
        }
    }
}
