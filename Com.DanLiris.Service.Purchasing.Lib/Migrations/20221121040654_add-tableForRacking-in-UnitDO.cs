using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addtableForRackinginUnitDO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "GarmentUnitDeliveryOrderItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Box",
                table: "GarmentUnitDeliveryOrderItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "GarmentUnitDeliveryOrderItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "GarmentUnitDeliveryOrderItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rack",
                table: "GarmentUnitDeliveryOrderItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "GarmentUnitDeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "Box",
                table: "GarmentUnitDeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "GarmentUnitDeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "GarmentUnitDeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "Rack",
                table: "GarmentUnitDeliveryOrderItems");
        }
    }
}
