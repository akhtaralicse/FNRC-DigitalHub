using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalHub.Domain.Migrations
{
    /// <inheritdoc />
    public partial class userroles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentNameAr",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DepartmentNameEn",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UAEPassEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserSOPType",
                table: "Users");

            migrationBuilder.AlterColumn<Guid>(
                name: "UId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentNameAr",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentNameEn",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdType",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UAEPassEmail",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserSOPType",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
