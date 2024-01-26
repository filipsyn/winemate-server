using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WineMate.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class AddWineMakersToDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wines_WineMaker_WineMakerId",
                table: "Wines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WineMaker",
                table: "WineMaker");

            migrationBuilder.RenameTable(
                name: "WineMaker",
                newName: "WineMakers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WineMakers",
                table: "WineMakers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Wines_WineMakers_WineMakerId",
                table: "Wines",
                column: "WineMakerId",
                principalTable: "WineMakers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wines_WineMakers_WineMakerId",
                table: "Wines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WineMakers",
                table: "WineMakers");

            migrationBuilder.RenameTable(
                name: "WineMakers",
                newName: "WineMaker");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WineMaker",
                table: "WineMaker",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Wines_WineMaker_WineMakerId",
                table: "Wines",
                column: "WineMakerId",
                principalTable: "WineMaker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
