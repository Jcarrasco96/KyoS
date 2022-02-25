using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class add_TCMStageV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Units = table.Column<int>(type: "int", nullable: false),
                    ID_Etapa = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tCMserviceId = table.Column<int>(type: "int", nullable: true),
                    ClinicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMStages_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TCMStages_TCMServices_tCMserviceId",
                        column: x => x.tCMserviceId,
                        principalTable: "TCMServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMStages_ClinicId",
                table: "TCMStages",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMStages_Name",
                table: "TCMStages",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMStages_tCMserviceId",
                table: "TCMStages",
                column: "tCMserviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMStages");
        }
    }
}
