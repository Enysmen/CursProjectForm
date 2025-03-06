using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedUserSurveyTemplate : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "SurveyTemplateId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SurveyTemplateId",
                table: "AspNetUsers",
                column: "SurveyTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SurveyTemplates_SurveyTemplateId",
                table: "AspNetUsers",
                column: "SurveyTemplateId",
                principalTable: "SurveyTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SurveyTemplates_SurveyTemplateId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SurveyTemplateId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AccessSetting",
                table: "SurveyTemplates");

            migrationBuilder.DropColumn(
                name: "SurveyTemplateId",
                table: "AspNetUsers");
        }
    }
}
