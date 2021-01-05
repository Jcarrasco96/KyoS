using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ChangeNamesOfEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes_Classifications");

            migrationBuilder.DropTable(
                name: "Notes_DailySessions_Clients");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.CreateTable(
                name: "NotesPrototypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnswerClient = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AnswerFacilitator = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotesPrototypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotesPrototypes_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotesPrototypes_Classifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoteId = table.Column<int>(type: "int", nullable: true),
                    ClassificationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotesPrototypes_Classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotesPrototypes_Classifications_Classifications_ClassificationId",
                        column: x => x.ClassificationId,
                        principalTable: "Classifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotesPrototypes_Classifications_NotesPrototypes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "NotesPrototypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotesPrototypes_DailySessions_Clients_Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoteId = table.Column<int>(type: "int", nullable: true),
                    DailySessionId = table.Column<int>(type: "int", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: true),
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
                name: "IX_NotesPrototypes_ActivityId",
                table: "NotesPrototypes",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesPrototypes_Classifications_ClassificationId",
                table: "NotesPrototypes_Classifications",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesPrototypes_Classifications_NoteId",
                table: "NotesPrototypes_Classifications",
                column: "NoteId");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotesPrototypes_Classifications");

            migrationBuilder.DropTable(
                name: "NotesPrototypes_DailySessions_Clients_Plans");

            migrationBuilder.DropTable(
                name: "NotesPrototypes");

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    AnswerClient = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AnswerFacilitator = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notes_Classifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassificationId = table.Column<int>(type: "int", nullable: true),
                    NoteId = table.Column<int>(type: "int", nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notes_DailySessions_Clients",
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
                    table.PrimaryKey("PK_Notes_DailySessions_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_DailySessions_Clients_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notes_DailySessions_Clients_DailySessions_DailySessionId",
                        column: x => x.DailySessionId,
                        principalTable: "DailySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notes_DailySessions_Clients_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notes_DailySessions_Clients_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_ActivityId",
                table: "Notes",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Classifications_ClassificationId",
                table: "Notes_Classifications",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Classifications_NoteId",
                table: "Notes_Classifications",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_DailySessions_Clients_ClientId",
                table: "Notes_DailySessions_Clients",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_DailySessions_Clients_DailySessionId",
                table: "Notes_DailySessions_Clients",
                column: "DailySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_DailySessions_Clients_NoteId",
                table: "Notes_DailySessions_Clients",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_DailySessions_Clients_PlanId",
                table: "Notes_DailySessions_Clients",
                column: "PlanId");
        }
    }
}
