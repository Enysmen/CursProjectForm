using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessSetting",
                table: "SurveyTemplates");
        }


    }
}
