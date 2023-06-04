using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class alter_Revaluations_add_Shop_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "Revaluations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Revaluations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Revaluations_ShopId",
                table: "Revaluations",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Revaluations_Shops_ShopId",
                table: "Revaluations",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Revaluations_Shops_ShopId",
                table: "Revaluations");

            migrationBuilder.DropIndex(
                name: "IX_Revaluations_ShopId",
                table: "Revaluations");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Revaluations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Revaluations");
        }
    }
}
