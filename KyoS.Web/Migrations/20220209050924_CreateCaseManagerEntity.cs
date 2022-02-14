using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class erwe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseMannager",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LinkedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignaturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClinicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseMannager", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseMannager_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseMannager_ClinicId",
                table: "CaseMannager",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseMannager_Name",
                table: "CaseMannager",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseMannager");
        }
    }
}
