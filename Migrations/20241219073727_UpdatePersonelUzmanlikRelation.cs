using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitness_Center_Web_Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonelUzmanlikRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Uzmanliklar_Personeller_PersonelId",
                table: "Uzmanliklar");

            migrationBuilder.DropIndex(
                name: "IX_Uzmanliklar_PersonelId",
                table: "Uzmanliklar");

            migrationBuilder.DropColumn(
                name: "PersonelId",
                table: "Uzmanliklar");

            migrationBuilder.CreateTable(
                name: "PersonelUzmanliklar",
                columns: table => new
                {
                    PersonellerId = table.Column<int>(type: "int", nullable: false),
                    UzmanliklarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelUzmanliklar", x => new { x.PersonellerId, x.UzmanliklarId });
                    table.ForeignKey(
                        name: "FK_PersonelUzmanliklar_Personeller_PersonellerId",
                        column: x => x.PersonellerId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonelUzmanliklar_Uzmanliklar_UzmanliklarId",
                        column: x => x.UzmanliklarId,
                        principalTable: "Uzmanliklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonelUzmanliklar_UzmanliklarId",
                table: "PersonelUzmanliklar",
                column: "UzmanliklarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonelUzmanliklar");

            migrationBuilder.AddColumn<int>(
                name: "PersonelId",
                table: "Uzmanliklar",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Uzmanliklar_PersonelId",
                table: "Uzmanliklar",
                column: "PersonelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Uzmanliklar_Personeller_PersonelId",
                table: "Uzmanliklar",
                column: "PersonelId",
                principalTable: "Personeller",
                principalColumn: "Id");
        }
    }
}
