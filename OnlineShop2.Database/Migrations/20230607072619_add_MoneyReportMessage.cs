using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class add_MoneyReportMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MoneyReportMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateRecive = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TypeDoc = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: false),
                    DocId = table.Column<int>(type: "integer", nullable: false),
                    Sum = table.Column<decimal>(type: "numeric", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyReportMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoneyReportMessages_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoneyReportMessages_ShopId",
                table: "MoneyReportMessages",
                column: "ShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoneyReportMessages");
        }
    }
}
