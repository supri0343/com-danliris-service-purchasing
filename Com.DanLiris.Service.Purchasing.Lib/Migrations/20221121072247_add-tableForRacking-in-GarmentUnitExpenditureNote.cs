using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addtableForRackinginGarmentUnitExpenditureNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "GarmentUnitExpenditureNoteItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Box",
                table: "GarmentUnitExpenditureNoteItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "GarmentUnitExpenditureNoteItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "GarmentUnitExpenditureNoteItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rack",
                table: "GarmentUnitExpenditureNoteItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "GarmentUnitExpenditureNoteItems");

            migrationBuilder.DropColumn(
                name: "Box",
                table: "GarmentUnitExpenditureNoteItems");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "GarmentUnitExpenditureNoteItems");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "GarmentUnitExpenditureNoteItems");

            migrationBuilder.DropColumn(
                name: "Rack",
                table: "GarmentUnitExpenditureNoteItems");
        }
    }
}
