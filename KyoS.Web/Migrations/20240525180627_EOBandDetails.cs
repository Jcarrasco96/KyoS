using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class EOBandDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EOBId",
                table: "Workdays_Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EOBId",
                table: "TCMNote",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EOBs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateBill = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateBillClose = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateBillPayment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Units = table.Column<int>(type: "int", nullable: false),
                    StatusBill = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EOBs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EOBDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EOBId = table.Column<int>(type: "int", nullable: true),
                    IdCLient = table.Column<int>(type: "int", nullable: false),
                    IdWorkddayClient = table.Column<int>(type: "int", nullable: false),
                    IdTCMNotes = table.Column<int>(type: "int", nullable: false),
                    ServiceAgency = table.Column<int>(type: "int", nullable: false),
                    DateService = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    StatusBill = table.Column<int>(type: "int", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NameClient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MedicaidBill = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EOBDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EOBDetails_EOBs_EOBId",
                        column: x => x.EOBId,
                        principalTable: "EOBs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Clients_EOBId",
                table: "Workdays_Clients",
                column: "EOBId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMNote_EOBId",
                table: "TCMNote",
                column: "EOBId");

            migrationBuilder.CreateIndex(
                name: "IX_EOBDetails_EOBId",
                table: "EOBDetails",
                column: "EOBId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_EOBs_EOBId",
                table: "TCMNote",
                column: "EOBId",
                principalTable: "EOBs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workdays_Clients_EOBs_EOBId",
                table: "Workdays_Clients",
                column: "EOBId",
                principalTable: "EOBs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_EOBs_EOBId",
                table: "TCMNote");

            migrationBuilder.DropForeignKey(
                name: "FK_Workdays_Clients_EOBs_EOBId",
                table: "Workdays_Clients");

            migrationBuilder.DropTable(
                name: "EOBDetails");

            migrationBuilder.DropTable(
                name: "EOBs");

            migrationBuilder.DropIndex(
                name: "IX_Workdays_Clients_EOBId",
                table: "Workdays_Clients");

            migrationBuilder.DropIndex(
                name: "IX_TCMNote_EOBId",
                table: "TCMNote");

            migrationBuilder.DropColumn(
                name: "EOBId",
                table: "Workdays_Clients");

            migrationBuilder.DropColumn(
                name: "EOBId",
                table: "TCMNote");
        }
    }
}
