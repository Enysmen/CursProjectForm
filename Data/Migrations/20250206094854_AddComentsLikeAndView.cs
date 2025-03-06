using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComentsLikeAndView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemplateComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurveyTemplateId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemplateComments_SurveyTemplates_SurveyTemplateId",
                        column: x => x.SurveyTemplateId,
                        principalTable: "SurveyTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TemplateLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurveyTemplateId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemplateLikes_SurveyTemplates_SurveyTemplateId",
                        column: x => x.SurveyTemplateId,
                        principalTable: "SurveyTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateComments_SurveyTemplateId",
                table: "TemplateComments",
                column: "SurveyTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateComments_UserId",
                table: "TemplateComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateLikes_SurveyTemplateId",
                table: "TemplateLikes",
                column: "SurveyTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateLikes_UserId",
                table: "TemplateLikes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateComments");

            migrationBuilder.DropTable(
                name: "TemplateLikes");
        }
    }
}
