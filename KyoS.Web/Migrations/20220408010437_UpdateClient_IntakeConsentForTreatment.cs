using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateClient_IntakeConsentForTreatment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeConsentForTreatment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuthorizeStaff = table.Column<bool>(type: "bit", nullable: false),
                    AuthorizeRelease = table.Column<bool>(type: "bit", nullable: false),
                    Underestand = table.Column<bool>(type: "bit", nullable: false),
                    Aggre = table.Column<bool>(type: "bit", nullable: false),
                    Aggre1 = table.Column<bool>(type: "bit", nullable: false),
                    Certify = table.Column<bool>(type: "bit", nullable: false),
                    Certify1 = table.Column<bool>(type: "bit", nullable: false),
                    Documents = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeConsentForTreatment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeConsentForTreatment_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeConsentForTreatment_Client_FK",
                table: "IntakeConsentForTreatment",
                column: "Client_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeConsentForTreatment");
        }
    }
}
