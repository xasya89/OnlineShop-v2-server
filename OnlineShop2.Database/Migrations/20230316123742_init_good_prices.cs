using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class init_good_prices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodPrice_Goods_GoodId",
                table: "GoodPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodPrice_Shops_ShopId",
                table: "GoodPrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GoodPrice",
                table: "GoodPrice");

            migrationBuilder.RenameTable(
                name: "GoodPrice",
                newName: "GoodPrices");

            migrationBuilder.RenameIndex(
                name: "IX_GoodPrice_ShopId",
                table: "GoodPrices",
                newName: "IX_GoodPrices_ShopId");

            migrationBuilder.RenameIndex(
                name: "IX_GoodPrice_GoodId",
                table: "GoodPrices",
                newName: "IX_GoodPrices_GoodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoodPrices",
                table: "GoodPrices",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodPrices_Goods_GoodId",
                table: "GoodPrices",
                column: "GoodId",
                principalTable: "Goods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodPrices_Shops_ShopId",
                table: "GoodPrices",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodPrices_Goods_GoodId",
                table: "GoodPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodPrices_Shops_ShopId",
                table: "GoodPrices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GoodPrices",
                table: "GoodPrices");

            migrationBuilder.RenameTable(
                name: "GoodPrices",
                newName: "GoodPrice");

            migrationBuilder.RenameIndex(
                name: "IX_GoodPrices_ShopId",
                table: "GoodPrice",
                newName: "IX_GoodPrice_ShopId");

            migrationBuilder.RenameIndex(
                name: "IX_GoodPrices_GoodId",
                table: "GoodPrice",
                newName: "IX_GoodPrice_GoodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoodPrice",
                table: "GoodPrice",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodPrice_Goods_GoodId",
                table: "GoodPrice",
                column: "GoodId",
                principalTable: "Goods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodPrice_Shops_ShopId",
                table: "GoodPrice",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
