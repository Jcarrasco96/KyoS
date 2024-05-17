using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class RealMinuteInAllNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Minute",
                table: "NotesP_Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minute",
                table: "Notes_Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minute",
                table: "IndividualNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RealUnits",
                table: "IndividualNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minute",
                table: "GroupNotes2_Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minute",
                table: "GroupNotes_Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Minute",
                table: "NotesP_Activities");

            migrationBuilder.DropColumn(
                name: "Minute",
                table: "Notes_Activities");

            migrationBuilder.DropColumn(
                name: "Minute",
                table: "IndividualNotes");

            migrationBuilder.DropColumn(
                name: "RealUnits",
                table: "IndividualNotes");

            migrationBuilder.DropColumn(
                name: "Minute",
                table: "GroupNotes2_Activities");

            migrationBuilder.DropColumn(
                name: "Minute",
                table: "GroupNotes_Activities");
        }
    }
}
