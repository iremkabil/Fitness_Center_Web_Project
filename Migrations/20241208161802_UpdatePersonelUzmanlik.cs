using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebProje.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonelUzmanlik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Islemler_Uzmanliklar_PersonelUzmanlikId",
                table: "Islemler");

            migrationBuilder.AlterColumn<int>(
                name: "PersonelUzmanlikId",
                table: "Islemler",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Islemler_Uzmanliklar_PersonelUzmanlikId",
                table: "Islemler",
                column: "PersonelUzmanlikId",
                principalTable: "Uzmanliklar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Islemler_Uzmanliklar_PersonelUzmanlikId",
                table: "Islemler");

            migrationBuilder.AlterColumn<int>(
                name: "PersonelUzmanlikId",
                table: "Islemler",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Islemler_Uzmanliklar_PersonelUzmanlikId",
                table: "Islemler",
                column: "PersonelUzmanlikId",
                principalTable: "Uzmanliklar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
