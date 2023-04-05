using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class alter_forgein_Goods_CurrentBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoodCurrentBalances_GoodId",
                table: "GoodCurrentBalances");

            migrationBuilder.CreateIndex(
                name: "IX_GoodCurrentBalances_GoodId",
                table: "GoodCurrentBalances",
                column: "GoodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoodCurrentBalances_GoodId",
                table: "GoodCurrentBalances");

            migrationBuilder.CreateIndex(
                name: "IX_GoodCurrentBalances_GoodId",
                table: "GoodCurrentBalances",
                column: "GoodId",
                unique: true);
        }
    }
}
