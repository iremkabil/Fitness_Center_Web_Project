using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitness_Center_Web_Project.Migrations
{
    /// <inheritdoc />
    public partial class randevuUpdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnayliMi",
                table: "Randevular");

            migrationBuilder.AddColumn<string>(
                name: "Durum",
                table: "Randevular",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Randevular");

            migrationBuilder.AddColumn<bool>(
                name: "OnayliMi",
                table: "Randevular",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
