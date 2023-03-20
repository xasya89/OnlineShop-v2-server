using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class add_InventorySummaryGood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM public.\"Inventories\"");
            migrationBuilder.CreateTable(
                name: "InventorySummaryGoods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    GoodId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    CountOld = table.Column<decimal>(type: "numeric", nullable: false),
                    CountCurrent = table.Column<decimal>(type: "numeric", nullable: false),
                    InventoryGroupId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySummaryGoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventorySummaryGoods_Goods_GoodId",
                        column: x => x.GoodId,
                        principalTable: "Goods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventorySummaryGoods_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventorySummaryGoods_InventoryGroups_InventoryGroupId",
                        column: x => x.InventoryGroupId,
                        principalTable: "InventoryGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventorySummaryGoods_GoodId",
                table: "InventorySummaryGoods",
                column: "GoodId");

            migrationBuilder.CreateIndex(
                name: "IX_InventorySummaryGoods_InventoryGroupId",
                table: "InventorySummaryGoods",
                column: "InventoryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_InventorySummaryGoods_InventoryId",
                table: "InventorySummaryGoods",
                column: "InventoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventorySummaryGoods");
        }
    }
}
