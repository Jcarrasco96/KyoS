using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddGenratedNotesEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotesPrototypes_DailySessions_Clients_Plans");

            migrationBuilder.CreateTable(
                name: "GeneratedNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailySessionId = table.Column<int>(type: "int", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    PlanId = table.Column<int>(type: "int", nullable: true),
                    OrientedX3 = table.Column<bool>(type: "bit", nullable: false),
                    NotTime = table.Column<bool>(type: "bit", nullable: false),
                    NotPlace = table.Column<bool>(type: "bit", nullable: false),
                    NotPerson = table.Column<bool>(type: "bit", nullable: false),
                    Present = table.Column<bool>(type: "bit", nullable: false),
                    Adequate = table.Column<bool>(type: "bit", nullable: false),
                    Limited = table.Column<bool>(type: "bit", nullable: false),
                    Impaired = table.Column<bool>(type: "bit", nullable: false),
                    Faulty = table.Column<bool>(type: "bit", nullable: false),
                    Euthymic = table.Column<bool>(type: "bit", nullable: false),
                    Congruent = table.Column<bool>(type: "bit", nullable: false),
                    Negativistic = table.Column<bool>(type: "bit", nullable: false),
                    Depressed = table.Column<bool>(type: "bit", nullable: false),
                    Euphoric = table.Column<bool>(type: "bit", nullable: false),
                    Optimistic = table.Column<bool>(type: "bit", nullable: false),
                    Anxious = table.Column<bool>(type: "bit", nullable: false),
                    Hostile = table.Column<bool>(type: "bit", nullable: false),
                    Withdrawn = table.Column<bool>(type: "bit", nullable: false),
                    Irritable = table.Column<bool>(type: "bit", nullable: false),
                    Dramatized = table.Column<bool>(type: "bit", nullable: false),
                    AdequateAC = table.Column<bool>(type: "bit", nullable: false),
                    Inadequate = table.Column<bool>(type: "bit", nullable: false),
                    Fair = table.Column<bool>(type: "bit", nullable: false),
                    Unmotivated = table.Column<bool>(type: "bit", nullable: false),
                    Motivated = table.Column<bool>(type: "bit", nullable: false),
                    Guarded = table.Column<bool>(type: "bit", nullable: false),
                    Normal = table.Column<bool>(type: "bit", nullable: false),
                    ShortSpanned = table.Column<bool>(type: "bit", nullable: false),
                    MildlyImpaired = table.Column<bool>(type: "bit", nullable: false),
                    SeverelyImpaired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedNotes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneratedNotes_DailySessions_DailySessionId",
                        column: x => x.DailySessionId,
                        principalTable: "DailySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneratedNotes_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedNotes_NotesPrototypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedNoteId = table.Column<int>(type: "int", nullable: true),
                    NotePrototypeId = table.Column<int>(type: "int", nullable: true),
                    LinkedGoal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedObj = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedNotes_NotesPrototypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedNotes_NotesPrototypes_GeneratedNotes_GeneratedNoteId",
                        column: x => x.GeneratedNoteId,
                        principalTable: "GeneratedNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneratedNotes_NotesPrototypes_NotesPrototypes_NotePrototypeId",
                        column: x => x.NotePrototypeId,
                        principalTable: "NotesPrototypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedNotes_ClientId",
                table: "GeneratedNotes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedNotes_DailySessionId",
                table: "GeneratedNotes",
                column: "DailySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedNotes_PlanId",
                table: "GeneratedNotes",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedNotes_NotesPrototypes_GeneratedNoteId",
                table: "GeneratedNotes_NotesPrototypes",
                column: "GeneratedNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedNotes_NotesPrototypes_NotePrototypeId",
                table: "GeneratedNotes_NotesPrototypes",
                column: "NotePrototypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedNotes_NotesPrototypes");

            migrationBuilder.DropTable(
                name: "GeneratedNotes");

            migrationBuilder.CreateTable(
                name: "NotesPrototypes_DailySessions_Clients_Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    DailySessionId = table.Column<int>(type: "int", nullable: true),
                    NoteId = table.Column<int>(type: "int", nullable: true),
                    PlanId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotesPrototypes_DailySessions_Clients_Plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotesPrototypes_DailySessions_Clients_Plans_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotesPrototypes_DailySessions_Clients_Plans_DailySessions_DailySessionId",
                        column: x => x.DailySessionId,
                        principalTable: "DailySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotesPrototypes_DailySessions_Clients_Plans_NotesPrototypes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "NotesPrototypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotesPrototypes_DailySessions_Clients_Plans_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotesPrototypes_DailySessions_Clients_Plans_ClientId",
                table: "NotesPrototypes_DailySessions_Clients_Plans",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesPrototypes_DailySessions_Clients_Plans_DailySessionId",
                table: "NotesPrototypes_DailySessions_Clients_Plans",
                column: "DailySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesPrototypes_DailySessions_Clients_Plans_NoteId",
                table: "NotesPrototypes_DailySessions_Clients_Plans",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesPrototypes_DailySessions_Clients_Plans_PlanId",
                table: "NotesPrototypes_DailySessions_Clients_Plans",
                column: "PlanId");
        }
    }
}
