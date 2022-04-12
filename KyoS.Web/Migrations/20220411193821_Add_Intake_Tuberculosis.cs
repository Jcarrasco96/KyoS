using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Add_Intake_Tuberculosis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeTuberculosis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoYouCurrently = table.Column<bool>(type: "bit", nullable: false),
                    DoYouBring = table.Column<bool>(type: "bit", nullable: false),
                    DoYouCough = table.Column<bool>(type: "bit", nullable: false),
                    DoYouSweat = table.Column<bool>(type: "bit", nullable: false),
                    DoYouHaveFever = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouLost = table.Column<bool>(type: "bit", nullable: false),
                    DoYouHaveChest = table.Column<bool>(type: "bit", nullable: false),
                    If2OrMore = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouRecently = table.Column<bool>(type: "bit", nullable: false),
                    AreYouRecently = table.Column<bool>(type: "bit", nullable: false),
                    IfYesWhich = table.Column<bool>(type: "bit", nullable: false),
                    DoYouOr = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverBeen = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverWorked = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverHadOrgan = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverConsidered = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverHadAbnormal = table.Column<bool>(type: "bit", nullable: false),
                    If3OrMore = table.Column<bool>(type: "bit", nullable: false),
                    HaveYouEverHadPositive = table.Column<bool>(type: "bit", nullable: false),
                    IfYesWhere = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    When = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HaveYoyEverBeenTold = table.Column<bool>(type: "bit", nullable: false),
                    AgencyExpectation = table.Column<bool>(type: "bit", nullable: false),
                    Education = table.Column<bool>(type: "bit", nullable: false),
                    TheAbove = table.Column<bool>(type: "bit", nullable: false),
                    If1OrMore = table.Column<bool>(type: "bit", nullable: false),
                    Documents = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeTuberculosis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeTuberculosis_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeTuberculosis_Client_FK",
                table: "IntakeTuberculosis",
                column: "Client_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeTuberculosis");
        }
    }
}
