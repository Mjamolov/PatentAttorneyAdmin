using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatentAttorneyAdmin.Data.Migrations
{
    public partial class AddUserPassportExtendedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActualAddress",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Inn",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PassportAddress",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportExpiryDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualAddress",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Inn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PassportAddress",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PassportExpiryDate",
                table: "AspNetUsers");
        }
    }
}
