using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddDailySessionEntityAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailySessions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Day = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    GroupId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailySessions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notes_DailySessions_Clients",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NoteId = table.Column<int>(nullable: true),
                    DailySessionId = table.Column<int>(nullable: true),
                    ClientId = table.Column<int>(nullable: true)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailySessions_GroupId",
                table: "DailySessions",
                column: "GroupId");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes_DailySessions_Clients");

            migrationBuilder.DropTable(
                name: "DailySessions");
        }
    }
}
