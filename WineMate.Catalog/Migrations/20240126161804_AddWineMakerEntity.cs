using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WineMate.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class AddWineMakerEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WineMakerId",
                table: "Wines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "WineMaker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address_Number = table.Column<int>(type: "integer", nullable: false),
                    Address_Street = table.Column<string>(type: "text", nullable: false),
                    Address_City = table.Column<string>(type: "text", nullable: false),
                    Address_Country = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WineMaker", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wines_WineMakerId",
                table: "Wines",
                column: "WineMakerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wines_WineMaker_WineMakerId",
                table: "Wines",
                column: "WineMakerId",
                principalTable: "WineMaker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wines_WineMaker_WineMakerId",
                table: "Wines");

            migrationBuilder.DropTable(
                name: "WineMaker");

            migrationBuilder.DropIndex(
                name: "IX_Wines_WineMakerId",
                table: "Wines");

            migrationBuilder.DropColumn(
                name: "WineMakerId",
                table: "Wines");
        }
    }
}
