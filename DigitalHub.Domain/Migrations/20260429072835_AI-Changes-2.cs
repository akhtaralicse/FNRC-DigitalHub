using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalHub.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AIChanges2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneratedQuestions",
                table: "AIAssistantDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedQuestions",
                table: "AIAssistantDocuments");
        }
    }
}
