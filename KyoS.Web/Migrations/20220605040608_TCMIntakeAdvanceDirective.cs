using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMIntakeAdvanceDirective : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeAdvancedDirective",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    DateSignatureLegalGuardian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignaturePerson = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSignatureEmployee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documents = table.Column<bool>(type: "bit", nullable: false),
                    IHave = table.Column<bool>(type: "bit", nullable: false),
                    IHaveNot = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeAdvancedDirective", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeAdvancedDirective_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeAdvancedDirective_TcmClient_FK",
                table: "TCMIntakeAdvancedDirective",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeAdvancedDirective");
        }
    }
}
