using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalHub.Domain.Migrations
{
    /// <inheritdoc />
    public partial class vidoinsecond : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VideoDisplaySec",
                table: "IconConfiguration",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoDisplaySec",
                table: "IconConfiguration");
        }
    }
}
