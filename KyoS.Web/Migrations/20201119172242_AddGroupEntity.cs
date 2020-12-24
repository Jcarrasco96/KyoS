using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddGroupEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Clients",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Am = table.Column<bool>(nullable: false),
                    Pm = table.Column<bool>(nullable: false),
                    FacilitatorEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Facilitators_FacilitatorEntityId",
                        column: x => x.FacilitatorEntityId,
                        principalTable: "Facilitators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_GroupId",
                table: "Clients",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_FacilitatorEntityId",
                table: "Groups",
                column: "FacilitatorEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Groups_GroupId",
                table: "Clients",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Groups_GroupId",
                table: "Clients");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Clients_GroupId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Clients");
        }
    }
}
