using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class alter_suppliers_groups_ADD_shopId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM public.\"Suppliers\"");
            migrationBuilder.Sql("DELETE FROM public.\"GoodsGroups\"");
            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "Suppliers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "GoodsGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_ShopId",
                table: "Suppliers",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsGroups_ShopId",
                table: "GoodsGroups",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsGroups_Shops_ShopId",
                table: "GoodsGroups",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Shops_ShopId",
                table: "Suppliers",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsGroups_Shops_ShopId",
                table: "GoodsGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Shops_ShopId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_ShopId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_GoodsGroups_ShopId",
                table: "GoodsGroups");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "GoodsGroups");
        }
    }
}
