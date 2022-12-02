using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addtableForRackinginGarmentStockOpnameItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "GarmentStockOpnameItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Box",
                table: "GarmentStockOpnameItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "GarmentStockOpnameItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "GarmentStockOpnameItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rack",
                table: "GarmentStockOpnameItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "GarmentStockOpnameItems");

            migrationBuilder.DropColumn(
                name: "Box",
                table: "GarmentStockOpnameItems");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "GarmentStockOpnameItems");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "GarmentStockOpnameItems");

            migrationBuilder.DropColumn(
                name: "Rack",
                table: "GarmentStockOpnameItems");
        }
    }
}
