using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class add_Revaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Revaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Create = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SumOld = table.Column<decimal>(type: "numeric", nullable: false),
                    SumNew = table.Column<decimal>(type: "numeric", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    DocumentType = table.Column<int>(type: "integer", nullable: true),
                    LegacyId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RevaluationGoods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RevaluationId = table.Column<int>(type: "integer", nullable: false),
                    GoodId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceOld = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceNew = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevaluationGoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevaluationGoods_Goods_GoodId",
                        column: x => x.GoodId,
                        principalTable: "Goods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevaluationGoods_Revaluations_RevaluationId",
                        column: x => x.RevaluationId,
                        principalTable: "Revaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevaluationGoods_GoodId",
                table: "RevaluationGoods",
                column: "GoodId");

            migrationBuilder.CreateIndex(
                name: "IX_RevaluationGoods_RevaluationId",
                table: "RevaluationGoods",
                column: "RevaluationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevaluationGoods");

            migrationBuilder.DropTable(
                name: "Revaluations");
        }
    }
}
