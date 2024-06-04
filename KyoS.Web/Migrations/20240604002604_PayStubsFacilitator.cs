using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class PayStubsFacilitator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PayStubId",
                table: "Workdays_Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayStubId",
                table: "MTPs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayStubId",
                table: "IntakeMedicalHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayStubId",
                table: "FarsForm",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayStubId",
                table: "Brief",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayStubId",
                table: "Bio",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PayStubs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatePayStub = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatePayStubClose = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatePayStubPayment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Units = table.Column<int>(type: "int", nullable: false),
                    StatusPayStub = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    FacilitatorId = table.Column<int>(type: "int", nullable: true),
                    Doc_AssisstantId = table.Column<int>(type: "int", nullable: true),
                    CantClient = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayStubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayStubs_DocumentsAssistant_Doc_AssisstantId",
                        column: x => x.Doc_AssisstantId,
                        principalTable: "DocumentsAssistant",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PayStubs_Facilitators_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalTable: "Facilitators",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayStubDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCMPayStubId = table.Column<int>(type: "int", nullable: true),
                    IdDocuAssisstant = table.Column<int>(type: "int", nullable: false),
                    IdWorkdayClient = table.Column<int>(type: "int", nullable: false),
                    IdFacilitator = table.Column<int>(type: "int", nullable: false),
                    DateService = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayStubDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayStubDetails_PayStubs_TCMPayStubId",
                        column: x => x.TCMPayStubId,
                        principalTable: "PayStubs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Clients_PayStubId",
                table: "Workdays_Clients",
                column: "PayStubId");

            migrationBuilder.CreateIndex(
                name: "IX_MTPs_PayStubId",
                table: "MTPs",
                column: "PayStubId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeMedicalHistory_PayStubId",
                table: "IntakeMedicalHistory",
                column: "PayStubId");

            migrationBuilder.CreateIndex(
                name: "IX_FarsForm_PayStubId",
                table: "FarsForm",
                column: "PayStubId");

            migrationBuilder.CreateIndex(
                name: "IX_Brief_PayStubId",
                table: "Brief",
                column: "PayStubId");

            migrationBuilder.CreateIndex(
                name: "IX_Bio_PayStubId",
                table: "Bio",
                column: "PayStubId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStubDetails_TCMPayStubId",
                table: "PayStubDetails",
                column: "TCMPayStubId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStubs_Doc_AssisstantId",
                table: "PayStubs",
                column: "Doc_AssisstantId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStubs_FacilitatorId",
                table: "PayStubs",
                column: "FacilitatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bio_PayStubs_PayStubId",
                table: "Bio",
                column: "PayStubId",
                principalTable: "PayStubs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Brief_PayStubs_PayStubId",
                table: "Brief",
                column: "PayStubId",
                principalTable: "PayStubs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FarsForm_PayStubs_PayStubId",
                table: "FarsForm",
                column: "PayStubId",
                principalTable: "PayStubs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeMedicalHistory_PayStubs_PayStubId",
                table: "IntakeMedicalHistory",
                column: "PayStubId",
                principalTable: "PayStubs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MTPs_PayStubs_PayStubId",
                table: "MTPs",
                column: "PayStubId",
                principalTable: "PayStubs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workdays_Clients_PayStubs_PayStubId",
                table: "Workdays_Clients",
                column: "PayStubId",
                principalTable: "PayStubs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bio_PayStubs_PayStubId",
                table: "Bio");

            migrationBuilder.DropForeignKey(
                name: "FK_Brief_PayStubs_PayStubId",
                table: "Brief");

            migrationBuilder.DropForeignKey(
                name: "FK_FarsForm_PayStubs_PayStubId",
                table: "FarsForm");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeMedicalHistory_PayStubs_PayStubId",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_MTPs_PayStubs_PayStubId",
                table: "MTPs");

            migrationBuilder.DropForeignKey(
                name: "FK_Workdays_Clients_PayStubs_PayStubId",
                table: "Workdays_Clients");

            migrationBuilder.DropTable(
                name: "PayStubDetails");

            migrationBuilder.DropTable(
                name: "PayStubs");

            migrationBuilder.DropIndex(
                name: "IX_Workdays_Clients_PayStubId",
                table: "Workdays_Clients");

            migrationBuilder.DropIndex(
                name: "IX_MTPs_PayStubId",
                table: "MTPs");

            migrationBuilder.DropIndex(
                name: "IX_IntakeMedicalHistory_PayStubId",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropIndex(
                name: "IX_FarsForm_PayStubId",
                table: "FarsForm");

            migrationBuilder.DropIndex(
                name: "IX_Brief_PayStubId",
                table: "Brief");

            migrationBuilder.DropIndex(
                name: "IX_Bio_PayStubId",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "PayStubId",
                table: "Workdays_Clients");

            migrationBuilder.DropColumn(
                name: "PayStubId",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "PayStubId",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "PayStubId",
                table: "FarsForm");

            migrationBuilder.DropColumn(
                name: "PayStubId",
                table: "Brief");

            migrationBuilder.DropColumn(
                name: "PayStubId",
                table: "Bio");
        }
    }
}
