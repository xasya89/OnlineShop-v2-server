using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class alter_MoneyReport_rename_StopGoodSum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartGoodsSum",
                table: "MoneyReports");

            migrationBuilder.AddColumn<decimal>(
                name: "StopGoodSum",
                table: "MoneyReports",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StopGoodSum",
                table: "MoneyReports");

            migrationBuilder.AddColumn<decimal>(
                name: "StartGoodsSum",
                table: "MoneyReports",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
