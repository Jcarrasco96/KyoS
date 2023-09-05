using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class BillDMS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillDmsId",
                table: "Workdays_Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillDmsId",
                table: "TCMNote",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BillDms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateBill = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateBillClose = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateBillPayment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Units = table.Column<int>(type: "int", nullable: false),
                    Different = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatusBill = table.Column<int>(type: "int", nullable: false),
                    FinishEdition = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillDms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillDmsDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillId = table.Column<int>(type: "int", nullable: true),
                    IdCLient = table.Column<int>(type: "int", nullable: false),
                    IdWorkddayClient = table.Column<int>(type: "int", nullable: false),
                    IdTCMNotes = table.Column<int>(type: "int", nullable: false),
                    ServiceAgency = table.Column<int>(type: "int", nullable: false),
                    DateService = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    StatusBill = table.Column<int>(type: "int", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NameClient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillDmsDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillDmsDetails_BillDms_BillId",
                        column: x => x.BillId,
                        principalTable: "BillDms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillDmsPaid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillId = table.Column<int>(type: "int", nullable: true),
                    DatePaid = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrigePaid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillDmsPaid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillDmsPaid_BillDms_BillId",
                        column: x => x.BillId,
                        principalTable: "BillDms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Clients_BillDmsId",
                table: "Workdays_Clients",
                column: "BillDmsId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMNote_BillDmsId",
                table: "TCMNote",
                column: "BillDmsId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDmsDetails_BillId",
                table: "BillDmsDetails",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDmsPaid_BillId",
                table: "BillDmsPaid",
                column: "BillId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_BillDms_BillDmsId",
                table: "TCMNote",
                column: "BillDmsId",
                principalTable: "BillDms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workdays_Clients_BillDms_BillDmsId",
                table: "Workdays_Clients",
                column: "BillDmsId",
                principalTable: "BillDms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_BillDms_BillDmsId",
                table: "TCMNote");

            migrationBuilder.DropForeignKey(
                name: "FK_Workdays_Clients_BillDms_BillDmsId",
                table: "Workdays_Clients");

            migrationBuilder.DropTable(
                name: "BillDmsDetails");

            migrationBuilder.DropTable(
                name: "BillDmsPaid");

            migrationBuilder.DropTable(
                name: "BillDms");

            migrationBuilder.DropIndex(
                name: "IX_Workdays_Clients_BillDmsId",
                table: "Workdays_Clients");

            migrationBuilder.DropIndex(
                name: "IX_TCMNote_BillDmsId",
                table: "TCMNote");

            migrationBuilder.DropColumn(
                name: "BillDmsId",
                table: "Workdays_Clients");

            migrationBuilder.DropColumn(
                name: "BillDmsId",
                table: "TCMNote");
        }
    }
}
