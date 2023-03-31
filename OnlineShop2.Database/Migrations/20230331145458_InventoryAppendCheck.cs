using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class InventoryAppendCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryAppendChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: false),
                    CheckSellId = table.Column<int>(type: "integer", nullable: false),
                    GoodId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<decimal>(type: "numeric", nullable: false),
                    InventoryGroupId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAppendChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryAppendChecks_CheckSells_CheckSellId",
                        column: x => x.CheckSellId,
                        principalTable: "CheckSells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAppendChecks_Goods_GoodId",
                        column: x => x.GoodId,
                        principalTable: "Goods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAppendChecks_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAppendChecks_InventoryGroups_InventoryGroupId",
                        column: x => x.InventoryGroupId,
                        principalTable: "InventoryGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryAppendChecks_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAppendChecks_CheckSellId",
                table: "InventoryAppendChecks",
                column: "CheckSellId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAppendChecks_GoodId",
                table: "InventoryAppendChecks",
                column: "GoodId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAppendChecks_InventoryGroupId",
                table: "InventoryAppendChecks",
                column: "InventoryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAppendChecks_InventoryId",
                table: "InventoryAppendChecks",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAppendChecks_ShopId",
                table: "InventoryAppendChecks",
                column: "ShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryAppendChecks");
        }
    }
}
