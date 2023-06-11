using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class alter_MoneyReport_add_Start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StartCashMoney",
                table: "MoneyReports",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StartGoodSum",
                table: "MoneyReports",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartCashMoney",
                table: "MoneyReports");

            migrationBuilder.DropColumn(
                name: "StartGoodSum",
                table: "MoneyReports");
        }
    }
}
