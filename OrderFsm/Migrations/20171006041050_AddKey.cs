using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace OrderFsm.Migrations
{
    public partial class AddKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "OrderProcesses");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "OrderProcesses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                table: "OrderProcesses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "OrderProcesses",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComplete",
                table: "OrderProcesses");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "OrderProcesses");

            migrationBuilder.AlterColumn<int>(
                name: "State",
                table: "OrderProcesses",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "OrderProcesses",
                nullable: true);
        }
    }
}
