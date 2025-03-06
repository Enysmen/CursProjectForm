using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldAccountIdSaleforceInApplicaUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalesforceAccountId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesforceAccountId",
                table: "AspNetUsers");
        }
    }
}
