using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ObjectiveTemp_for_CreateMTP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObjetivesTemp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Objetive = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOpened = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTarget = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateResolved = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Intervention = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoalTempId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjetivesTemp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjetivesTemp_GoalsTemp_GoalTempId",
                        column: x => x.GoalTempId,
                        principalTable: "GoalsTemp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObjetivesTemp_GoalTempId",
                table: "ObjetivesTemp",
                column: "GoalTempId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjetivesTemp");
        }
    }
}
