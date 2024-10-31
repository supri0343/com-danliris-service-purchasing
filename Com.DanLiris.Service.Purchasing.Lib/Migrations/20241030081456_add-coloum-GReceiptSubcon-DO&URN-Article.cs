using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addcoloumGReceiptSubconDOURNArticle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Article",
                table: "GarmentSubconUnitReceiptNoteItems",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Article",
                table: "GarmentSubconDeliveryOrderItems",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Article",
                table: "GarmentSubconUnitReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "Article",
                table: "GarmentSubconDeliveryOrderItems");
        }
    }
}
