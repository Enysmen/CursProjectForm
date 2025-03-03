using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedUuserAccessString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessSetting",
                table: "SurveyTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessSetting",
                table: "SurveyTemplates");
        }
    }
}
