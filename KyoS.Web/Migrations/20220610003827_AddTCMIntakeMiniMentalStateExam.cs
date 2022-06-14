using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddTCMIntakeMiniMentalStateExam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeMiniMental",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrientationWhat = table.Column<int>(type: "int", nullable: false),
                    OrientationWhere = table.Column<int>(type: "int", nullable: false),
                    RegistrationName = table.Column<int>(type: "int", nullable: false),
                    Attention = table.Column<int>(type: "int", nullable: false),
                    Recall = table.Column<int>(type: "int", nullable: false),
                    LanguageName = table.Column<int>(type: "int", nullable: false),
                    LanguageRepeat = table.Column<int>(type: "int", nullable: false),
                    LanguageFollow = table.Column<int>(type: "int", nullable: false),
                    LanguageRead = table.Column<int>(type: "int", nullable: false),
                    LanguageWrite = table.Column<int>(type: "int", nullable: false),
                    LanguageCopy = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeMiniMental", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeMiniMental_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeMiniMental_TcmClient_FK",
                table: "TCMIntakeMiniMental",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeMiniMental");
        }
    }
}
