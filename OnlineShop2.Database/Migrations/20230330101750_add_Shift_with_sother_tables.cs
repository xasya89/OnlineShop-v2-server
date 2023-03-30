using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class add_Shift_with_sother_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Stop",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Start",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShopId = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Stop = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SumAll = table.Column<decimal>(type: "numeric", nullable: false),
                    SumNoElectron = table.Column<decimal>(type: "numeric", nullable: false),
                    SumElectron = table.Column<decimal>(type: "numeric", nullable: false),
                    SumSell = table.Column<decimal>(type: "numeric", nullable: false),
                    SumDiscount = table.Column<decimal>(type: "numeric", nullable: false),
                    SumReturnNoElectron = table.Column<decimal>(type: "numeric", nullable: false),
                    SumReturnElectron = table.Column<decimal>(type: "numeric", nullable: false),
                    SumIncome = table.Column<decimal>(type: "numeric", nullable: false),
                    SumOutcome = table.Column<decimal>(type: "numeric", nullable: false),
                    CashierName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckSells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    DateCreate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TypeSell = table.Column<int>(type: "integer", nullable: false),
                    BuyerId = table.Column<int>(type: "integer", nullable: true),
                    BuyerName = table.Column<string>(type: "text", nullable: true),
                    BuyerPhone = table.Column<string>(type: "text", nullable: true),
                    SumBuy = table.Column<decimal>(type: "numeric", nullable: false),
                    SumDiscont = table.Column<decimal>(type: "numeric", nullable: false),
                    SumNoElectron = table.Column<decimal>(type: "numeric", nullable: false),
                    SumElectron = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckSells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckSells_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    GoodId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<double>(type: "double precision", nullable: false),
                    Sum = table.Column<decimal>(type: "numeric", nullable: false),
                    CountReturn = table.Column<decimal>(type: "numeric", nullable: false),
                    SumReturn = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftSummaries_Goods_GoodId",
                        column: x => x.GoodId,
                        principalTable: "Goods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftSummaries_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckGoods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CheckSellId = table.Column<int>(type: "integer", nullable: false),
                    GoodId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<decimal>(type: "numeric", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckGoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckGoods_CheckSells_CheckSellId",
                        column: x => x.CheckSellId,
                        principalTable: "CheckSells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CheckGoods_Goods_GoodId",
                        column: x => x.GoodId,
                        principalTable: "Goods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckGoods_CheckSellId",
                table: "CheckGoods",
                column: "CheckSellId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckGoods_GoodId",
                table: "CheckGoods",
                column: "GoodId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckSells_ShiftId",
                table: "CheckSells",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_ShopId",
                table: "Shifts",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSummaries_GoodId",
                table: "ShiftSummaries",
                column: "GoodId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSummaries_ShiftId",
                table: "ShiftSummaries",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckGoods");

            migrationBuilder.DropTable(
                name: "ShiftSummaries");

            migrationBuilder.DropTable(
                name: "CheckSells");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RefreshTokens",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Stop",
                table: "Inventories",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Start",
                table: "Inventories",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
