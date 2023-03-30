using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class alter_Supplier_GoodGroup_add_LegacyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LegacyId",
                table: "Suppliers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LegacyId",
                table: "GoodsGroups",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_LegacyId",
                table: "Suppliers",
                column: "LegacyId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsGroups_LegacyId",
                table: "GoodsGroups",
                column: "LegacyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suppliers_LegacyId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_GoodsGroups_LegacyId",
                table: "GoodsGroups");

            migrationBuilder.DropColumn(
                name: "LegacyId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "LegacyId",
                table: "GoodsGroups");
        }
    }
}
