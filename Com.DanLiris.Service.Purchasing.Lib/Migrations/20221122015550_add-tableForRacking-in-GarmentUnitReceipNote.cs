using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addtableForRackinginGarmentUnitReceipNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "GarmentUnitReceiptNoteItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Box",
                table: "GarmentUnitReceiptNoteItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "GarmentUnitReceiptNoteItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "GarmentUnitReceiptNoteItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rack",
                table: "GarmentUnitReceiptNoteItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "GarmentUnitReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "Box",
                table: "GarmentUnitReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "GarmentUnitReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "GarmentUnitReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "Rack",
                table: "GarmentUnitReceiptNoteItems");
        }
    }
}
