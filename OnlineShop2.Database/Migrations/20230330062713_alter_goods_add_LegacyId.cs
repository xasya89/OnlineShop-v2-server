using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class alter_goods_add_LegacyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<int>(
                name: "LegacyId",
                table: "Goods",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goods_LegacyId",
                table: "Goods",
                column: "LegacyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Goods_LegacyId",
                table: "Goods");

            migrationBuilder.DropColumn(
                name: "LegacyId",
                table: "Goods");
        }
    }
}
