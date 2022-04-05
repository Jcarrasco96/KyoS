using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMDischargeAndTCMDischargeServiceStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMDischargeServiceStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    TcmDischargeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMDischargeServiceStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMDischargeServiceStatus_TCMDischarge_TcmDischargeId",
                        column: x => x.TcmDischargeId,
                        principalTable: "TCMDischarge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMDischargeServiceStatus_TcmDischargeId",
                table: "TCMDischargeServiceStatus",
                column: "TcmDischargeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMDischargeServiceStatus");
        }
    }
}
