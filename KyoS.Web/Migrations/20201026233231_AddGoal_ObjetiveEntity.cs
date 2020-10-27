using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddGoal_ObjetiveEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "NumberOfMonths",
                table: "MTPs",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateTable(
                name: "GoalEntity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    AreaOfFocus = table.Column<string>(nullable: false),
                    MTPId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalEntity_MTPs_MTPId",
                        column: x => x.MTPId,
                        principalTable: "MTPs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ObjetiveEntity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Objetive = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    DateOpened = table.Column<DateTime>(nullable: false),
                    DateTarget = table.Column<DateTime>(nullable: false),
                    DateResolved = table.Column<DateTime>(nullable: false),
                    Intervention = table.Column<string>(nullable: false),
                    GoalId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjetiveEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjetiveEntity_GoalEntity_GoalId",
                        column: x => x.GoalId,
                        principalTable: "GoalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoalEntity_MTPId",
                table: "GoalEntity",
                column: "MTPId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjetiveEntity_GoalId",
                table: "ObjetiveEntity",
                column: "GoalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjetiveEntity");

            migrationBuilder.DropTable(
                name: "GoalEntity");

            migrationBuilder.AlterColumn<int>(
                name: "NumberOfMonths",
                table: "MTPs",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
