using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class addColumnGBeacukaiForSubcon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinishedGoodType",
                table: "GarmentBeacukais",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "QuantityContract",
                table: "GarmentBeacukais",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "SubconContractId",
                table: "GarmentBeacukais",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SubconContractNo",
                table: "GarmentBeacukais",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishedGoodType",
                table: "GarmentBeacukais");

            migrationBuilder.DropColumn(
                name: "QuantityContract",
                table: "GarmentBeacukais");

            migrationBuilder.DropColumn(
                name: "SubconContractId",
                table: "GarmentBeacukais");

            migrationBuilder.DropColumn(
                name: "SubconContractNo",
                table: "GarmentBeacukais");
        }
    }
}
