using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateClient_IntakeConsentPhotograph : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeConsentPhotograph",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Photograph = table.Column<bool>(type: "bit", nullable: false),
                    Filmed = table.Column<bool>(type: "bit", nullable: false),
                    VideoTaped = table.Column<bool>(type: "bit", nullable: false),
                    Interviwed = table.Column<bool>(type: "bit", nullable: false),
                    NoneOfTheForegoing = table.Column<bool>(type: "bit", nullable: false),
                    Publication = table.Column<bool>(type: "bit", nullable: false),
                    Broadcast = table.Column<bool>(type: "bit", nullable: false),
                    Markrting = table.Column<bool>(type: "bit", nullable: false),
                    ByTODocument = table.Column<bool>(type: "bit", nullable: false),
                    Documents = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeConsentPhotograph", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeConsentPhotograph_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeConsentPhotograph_Client_FK",
                table: "IntakeConsentPhotograph",
                column: "Client_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeConsentPhotograph");
        }
    }
}
