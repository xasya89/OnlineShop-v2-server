using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Writeof : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodPrices_Goods_GoodId",
                table: "GoodPrices");

            migrationBuilder.AlterColumn<int>(
                name: "GoodId",
                table: "GoodPrices",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Writeofs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DateWriteof = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    SumAll = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Writeofs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Writeofs_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WriteofGoods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WriteofId = table.Column<int>(type: "integer", nullable: false),
                    GoodId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Count = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WriteofGoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WriteofGoods_Goods_GoodId",
                        column: x => x.GoodId,
                        principalTable: "Goods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WriteofGoods_Writeofs_WriteofId",
                        column: x => x.WriteofId,
                        principalTable: "Writeofs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WriteofGoods_GoodId",
                table: "WriteofGoods",
                column: "GoodId");

            migrationBuilder.CreateIndex(
                name: "IX_WriteofGoods_WriteofId",
                table: "WriteofGoods",
                column: "WriteofId");

            migrationBuilder.CreateIndex(
                name: "IX_Writeofs_ShopId",
                table: "Writeofs",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodPrices_Goods_GoodId",
                table: "GoodPrices",
                column: "GoodId",
                principalTable: "Goods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodPrices_Goods_GoodId",
                table: "GoodPrices");

            migrationBuilder.DropTable(
                name: "WriteofGoods");

            migrationBuilder.DropTable(
                name: "Writeofs");

            migrationBuilder.AlterColumn<int>(
                name: "GoodId",
                table: "GoodPrices",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodPrices_Goods_GoodId",
                table: "GoodPrices",
                column: "GoodId",
                principalTable: "Goods",
                principalColumn: "Id");
        }
    }
}
