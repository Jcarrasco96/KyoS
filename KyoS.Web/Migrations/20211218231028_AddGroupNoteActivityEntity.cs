using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddGroupNoteActivityEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Activities_GroupNotes_GroupNoteId",
                table: "Notes_Activities");

            migrationBuilder.RenameColumn(
                name: "GroupNoteId",
                table: "Notes_Activities",
                newName: "GroupNoteEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_Activities_GroupNoteId",
                table: "Notes_Activities",
                newName: "IX_Notes_Activities_GroupNoteEntityId");

            migrationBuilder.CreateTable(
                name: "GroupNotes_Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupNoteId = table.Column<int>(type: "int", nullable: true),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    AnswerClient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnswerFacilitator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObjetiveId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupNotes_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupNotes_Activities_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupNotes_Activities_GroupNotes_GroupNoteId",
                        column: x => x.GroupNoteId,
                        principalTable: "GroupNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupNotes_Activities_Objetives_ObjetiveId",
                        column: x => x.ObjetiveId,
                        principalTable: "Objetives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes_Activities_ActivityId",
                table: "GroupNotes_Activities",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes_Activities_GroupNoteId",
                table: "GroupNotes_Activities",
                column: "GroupNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotes_Activities_ObjetiveId",
                table: "GroupNotes_Activities",
                column: "ObjetiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Activities_GroupNotes_GroupNoteEntityId",
                table: "Notes_Activities",
                column: "GroupNoteEntityId",
                principalTable: "GroupNotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Activities_GroupNotes_GroupNoteEntityId",
                table: "Notes_Activities");

            migrationBuilder.DropTable(
                name: "GroupNotes_Activities");

            migrationBuilder.RenameColumn(
                name: "GroupNoteEntityId",
                table: "Notes_Activities",
                newName: "GroupNoteId");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_Activities_GroupNoteEntityId",
                table: "Notes_Activities",
                newName: "IX_Notes_Activities_GroupNoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Activities_GroupNotes_GroupNoteId",
                table: "Notes_Activities",
                column: "GroupNoteId",
                principalTable: "GroupNotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
