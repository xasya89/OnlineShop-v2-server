using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class alter_Shop_add_LegacayDbNum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LegacyDbNum",
                table: "Shops",
                type: "integer",
                nullable: true);
            migrationBuilder.Sql("UPDATE public.\"Shops\" SET \"LegacyDbNum\"=7 WHERE \"Alias\"='Северный'");
            migrationBuilder.Sql("UPDATE public.\"Shops\" SET \"LegacyDbNum\"=3 WHERE \"Alias\"='Степная'");
            migrationBuilder.Sql("UPDATE public.\"Shops\" SET \"LegacyDbNum\"=2 WHERE \"Alias\"='Разумное'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacyDbNum",
                table: "Shops");
        }
    }
}
