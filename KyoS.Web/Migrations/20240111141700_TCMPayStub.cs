using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMPayStub : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PayStubId",
                table: "TCMNote",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TCMPayStubEntityId",
                table: "BillDmsDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TCMPayStubs",
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
                    CaseMannagerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMPayStubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMPayStubs_CaseManagers_CaseMannagerId",
                        column: x => x.CaseMannagerId,
                        principalTable: "CaseManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TCMPayStubDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillId = table.Column<int>(type: "int", nullable: true),
                    IdCaseManager = table.Column<int>(type: "int", nullable: false),
                    IdTCMNotes = table.Column<int>(type: "int", nullable: false),
                    DateService = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMPayStubDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMPayStubDetails_TCMPayStubs_BillId",
                        column: x => x.BillId,
                        principalTable: "TCMPayStubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMNote_PayStubId",
                table: "TCMNote",
                column: "PayStubId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDmsDetails_TCMPayStubEntityId",
                table: "BillDmsDetails",
                column: "TCMPayStubEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMPayStubDetails_BillId",
                table: "TCMPayStubDetails",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMPayStubs_CaseMannagerId",
                table: "TCMPayStubs",
                column: "CaseMannagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillDmsDetails_TCMPayStubs_TCMPayStubEntityId",
                table: "BillDmsDetails",
                column: "TCMPayStubEntityId",
                principalTable: "TCMPayStubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_TCMPayStubs_PayStubId",
                table: "TCMNote",
                column: "PayStubId",
                principalTable: "TCMPayStubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillDmsDetails_TCMPayStubs_TCMPayStubEntityId",
                table: "BillDmsDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_TCMPayStubs_PayStubId",
                table: "TCMNote");

            migrationBuilder.DropTable(
                name: "TCMPayStubDetails");

            migrationBuilder.DropTable(
                name: "TCMPayStubs");

            migrationBuilder.DropIndex(
                name: "IX_TCMNote_PayStubId",
                table: "TCMNote");

            migrationBuilder.DropIndex(
                name: "IX_BillDmsDetails_TCMPayStubEntityId",
                table: "BillDmsDetails");

            migrationBuilder.DropColumn(
                name: "PayStubId",
                table: "TCMNote");

            migrationBuilder.DropColumn(
                name: "TCMPayStubEntityId",
                table: "BillDmsDetails");
        }
    }
}
