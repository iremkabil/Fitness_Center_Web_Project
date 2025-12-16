using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebProje.Migrations
{
    /// <inheritdoc />
    public partial class IslemUzmanlikEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Islemler_Uzmanliklar_PersonelUzmanlikId",
                table: "Islemler");

            migrationBuilder.RenameColumn(
                name: "PersonelUzmanlikId",
                table: "Islemler",
                newName: "UzmanlikId");

            migrationBuilder.RenameIndex(
                name: "IX_Islemler_PersonelUzmanlikId",
                table: "Islemler",
                newName: "IX_Islemler_UzmanlikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Islemler_Uzmanliklar_UzmanlikId",
                table: "Islemler",
                column: "UzmanlikId",
                principalTable: "Uzmanliklar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Islemler_Uzmanliklar_UzmanlikId",
                table: "Islemler");

            migrationBuilder.RenameColumn(
                name: "UzmanlikId",
                table: "Islemler",
                newName: "PersonelUzmanlikId");

            migrationBuilder.RenameIndex(
                name: "IX_Islemler_UzmanlikId",
                table: "Islemler",
                newName: "IX_Islemler_PersonelUzmanlikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Islemler_Uzmanliklar_PersonelUzmanlikId",
                table: "Islemler",
                column: "PersonelUzmanlikId",
                principalTable: "Uzmanliklar",
                principalColumn: "Id");
        }
    }
}
