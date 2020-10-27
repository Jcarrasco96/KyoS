using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AddObjetive_ClassificationEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalEntity_MTPs_MTPId",
                table: "GoalEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_ObjetiveEntity_GoalEntity_GoalId",
                table: "ObjetiveEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjetiveEntity",
                table: "ObjetiveEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GoalEntity",
                table: "GoalEntity");

            migrationBuilder.RenameTable(
                name: "ObjetiveEntity",
                newName: "Objetives");

            migrationBuilder.RenameTable(
                name: "GoalEntity",
                newName: "Goals");

            migrationBuilder.RenameIndex(
                name: "IX_ObjetiveEntity_GoalId",
                table: "Objetives",
                newName: "IX_Objetives_GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_GoalEntity_MTPId",
                table: "Goals",
                newName: "IX_Goals_MTPId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Objetives",
                table: "Objetives",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Goals",
                table: "Goals",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ClassificationEntity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificationEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Objetives_Classifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ObjetiveId = table.Column<int>(nullable: true),
                    ClassificationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objetives_Classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Objetives_Classifications_ClassificationEntity_ClassificationId",
                        column: x => x.ClassificationId,
                        principalTable: "ClassificationEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Objetives_Classifications_Objetives_ObjetiveId",
                        column: x => x.ObjetiveId,
                        principalTable: "Objetives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Objetives_Classifications_ClassificationId",
                table: "Objetives_Classifications",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Objetives_Classifications_ObjetiveId",
                table: "Objetives_Classifications",
                column: "ObjetiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_MTPs_MTPId",
                table: "Goals",
                column: "MTPId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Objetives_Goals_GoalId",
                table: "Objetives",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_MTPs_MTPId",
                table: "Goals");

            migrationBuilder.DropForeignKey(
                name: "FK_Objetives_Goals_GoalId",
                table: "Objetives");

            migrationBuilder.DropTable(
                name: "Objetives_Classifications");

            migrationBuilder.DropTable(
                name: "ClassificationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Objetives",
                table: "Objetives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Goals",
                table: "Goals");

            migrationBuilder.RenameTable(
                name: "Objetives",
                newName: "ObjetiveEntity");

            migrationBuilder.RenameTable(
                name: "Goals",
                newName: "GoalEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Objetives_GoalId",
                table: "ObjetiveEntity",
                newName: "IX_ObjetiveEntity_GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_Goals_MTPId",
                table: "GoalEntity",
                newName: "IX_GoalEntity_MTPId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjetiveEntity",
                table: "ObjetiveEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoalEntity",
                table: "GoalEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalEntity_MTPs_MTPId",
                table: "GoalEntity",
                column: "MTPId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ObjetiveEntity_GoalEntity_GoalId",
                table: "ObjetiveEntity",
                column: "GoalId",
                principalTable: "GoalEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
