using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class alter_ShiftSummary_CheckSell_add_LegacyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LegacyId",
                table: "ShiftSummaries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LegacyId",
                table: "CheckSells",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacyId",
                table: "ShiftSummaries");

            migrationBuilder.DropColumn(
                name: "LegacyId",
                table: "CheckSells");
        }
    }
}
