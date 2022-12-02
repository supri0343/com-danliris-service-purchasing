using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class AddTableForRackingDOItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Box",
                table: "GarmentDOItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "GarmentDOItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "GarmentDOItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rack",
                table: "GarmentDOItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "GarmentDOItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Box",
                table: "GarmentDOItems");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "GarmentDOItems");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "GarmentDOItems");

            migrationBuilder.DropColumn(
                name: "Rack",
                table: "GarmentDOItems");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "GarmentDOItems");
        }
    }
}
