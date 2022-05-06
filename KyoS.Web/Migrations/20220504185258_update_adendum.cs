using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class update_adendum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adendum_Facilitators_FacilitatorId",
                table: "Adendum");

            migrationBuilder.DropForeignKey(
                name: "FK_Adendum_MTPs_MtpId",
                table: "Adendum");

            migrationBuilder.DropForeignKey(
                name: "FK_Adendum_Supervisors_SupervisorId",
                table: "Adendum");

            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Adendum_AdendumEntityId",
                table: "Goals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Adendum",
                table: "Adendum");

            migrationBuilder.RenameTable(
                name: "Adendum",
                newName: "Adendums");

            migrationBuilder.RenameIndex(
                name: "IX_Adendum_SupervisorId",
                table: "Adendums",
                newName: "IX_Adendums_SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_Adendum_MtpId",
                table: "Adendums",
                newName: "IX_Adendums_MtpId");

            migrationBuilder.RenameIndex(
                name: "IX_Adendum_FacilitatorId",
                table: "Adendums",
                newName: "IX_Adendums_FacilitatorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Adendums",
                table: "Adendums",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Adendums_Facilitators_FacilitatorId",
                table: "Adendums",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Adendums_MTPs_MtpId",
                table: "Adendums",
                column: "MtpId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Adendums_Supervisors_SupervisorId",
                table: "Adendums",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Adendums_AdendumEntityId",
                table: "Goals",
                column: "AdendumEntityId",
                principalTable: "Adendums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adendums_Facilitators_FacilitatorId",
                table: "Adendums");

            migrationBuilder.DropForeignKey(
                name: "FK_Adendums_MTPs_MtpId",
                table: "Adendums");

            migrationBuilder.DropForeignKey(
                name: "FK_Adendums_Supervisors_SupervisorId",
                table: "Adendums");

            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Adendums_AdendumEntityId",
                table: "Goals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Adendums",
                table: "Adendums");

            migrationBuilder.RenameTable(
                name: "Adendums",
                newName: "Adendum");

            migrationBuilder.RenameIndex(
                name: "IX_Adendums_SupervisorId",
                table: "Adendum",
                newName: "IX_Adendum_SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_Adendums_MtpId",
                table: "Adendum",
                newName: "IX_Adendum_MtpId");

            migrationBuilder.RenameIndex(
                name: "IX_Adendums_FacilitatorId",
                table: "Adendum",
                newName: "IX_Adendum_FacilitatorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Adendum",
                table: "Adendum",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Adendum_Facilitators_FacilitatorId",
                table: "Adendum",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Adendum_MTPs_MtpId",
                table: "Adendum",
                column: "MtpId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Adendum_Supervisors_SupervisorId",
                table: "Adendum",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Adendum_AdendumEntityId",
                table: "Goals",
                column: "AdendumEntityId",
                principalTable: "Adendum",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
