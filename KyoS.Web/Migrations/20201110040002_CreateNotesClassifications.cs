using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class CreateNotesClassifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Clasificacion",
                table: "Notes");

            migrationBuilder.CreateTable(
                name: "Notes_Classifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NoteId = table.Column<int>(nullable: true),
                    ClassificationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes_Classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Classifications_Classifications_ClassificationId",
                        column: x => x.ClassificationId,
                        principalTable: "Classifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notes_Classifications_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Classifications_ClassificationId",
                table: "Notes_Classifications",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Classifications_NoteId",
                table: "Notes_Classifications",
                column: "NoteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes_Classifications");

            migrationBuilder.AddColumn<int>(
                name: "Clasificacion",
                table: "Notes",
                nullable: false,
                defaultValue: 0);
        }
    }
}
