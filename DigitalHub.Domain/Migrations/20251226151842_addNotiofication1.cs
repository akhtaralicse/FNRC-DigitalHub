using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalHub.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addNotiofication1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionText",
                table: "NotificationConfiguration");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "NotificationConfiguration",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "TargetingCriteria",
                table: "NotificationConfiguration",
                newName: "TitleAr");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "NotificationConfiguration",
                newName: "MessageEn");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "NotificationConfiguration",
                newName: "MessageAr");

            migrationBuilder.RenameColumn(
                name: "DetailedDescription",
                table: "NotificationConfiguration",
                newName: "ActionTextEn");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "NotificationConfiguration",
                newName: "ActionTextAr");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TitleEn",
                table: "NotificationConfiguration",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "TitleAr",
                table: "NotificationConfiguration",
                newName: "TargetingCriteria");

            migrationBuilder.RenameColumn(
                name: "MessageEn",
                table: "NotificationConfiguration",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "MessageAr",
                table: "NotificationConfiguration",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ActionTextEn",
                table: "NotificationConfiguration",
                newName: "DetailedDescription");

            migrationBuilder.RenameColumn(
                name: "ActionTextAr",
                table: "NotificationConfiguration",
                newName: "Category");

            migrationBuilder.AddColumn<string>(
                name: "ActionText",
                table: "NotificationConfiguration",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
