using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class IntakeConsentForTelehealth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeConsentForTelehealth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IConsentToReceive = table.Column<bool>(type: "bit", nullable: false),
                    Documents = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeConsentForTelehealth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeConsentForTelehealth_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeConsentForTelehealth_Client_FK",
                table: "IntakeConsentForTelehealth",
                column: "Client_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeConsentForTelehealth");
        }
    }
}
