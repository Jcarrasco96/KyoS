using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMAdendumVersion1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Adendum",
                table: "TCMObjetives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Adendum",
                table: "TCMDomains",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TCMAdendumEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateIdentified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NeedsIdentified = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TcmServicePlanId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMAdendumEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMAdendumEntity_TCMServicePlans_TcmServicePlanId",
                        column: x => x.TcmServicePlanId,
                        principalTable: "TCMServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMAdendumEntity_TcmServicePlanId",
                table: "TCMAdendumEntity",
                column: "TcmServicePlanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMAdendumEntity");

            migrationBuilder.DropColumn(
                name: "Adendum",
                table: "TCMObjetives");

            migrationBuilder.DropColumn(
                name: "Adendum",
                table: "TCMDomains");
        }
    }
}
