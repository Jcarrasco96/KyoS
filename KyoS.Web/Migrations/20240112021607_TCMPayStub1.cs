using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMPayStub1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillDmsDetails_TCMPayStubs_TCMPayStubEntityId",
                table: "BillDmsDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMPayStubDetails_TCMPayStubs_BillId",
                table: "TCMPayStubDetails");

            migrationBuilder.DropIndex(
                name: "IX_BillDmsDetails_TCMPayStubEntityId",
                table: "BillDmsDetails");

            migrationBuilder.DropColumn(
                name: "TCMPayStubEntityId",
                table: "BillDmsDetails");

            migrationBuilder.RenameColumn(
                name: "BillId",
                table: "TCMPayStubDetails",
                newName: "TCMPayStubId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMPayStubDetails_BillId",
                table: "TCMPayStubDetails",
                newName: "IX_TCMPayStubDetails_TCMPayStubId");

            migrationBuilder.AddColumn<string>(
                name: "NameClient",
                table: "TCMPayStubDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TCMPayStub_Filtro",
                table: "Settings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMPayStubDetails_TCMPayStubs_TCMPayStubId",
                table: "TCMPayStubDetails",
                column: "TCMPayStubId",
                principalTable: "TCMPayStubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMPayStubDetails_TCMPayStubs_TCMPayStubId",
                table: "TCMPayStubDetails");

            migrationBuilder.DropColumn(
                name: "NameClient",
                table: "TCMPayStubDetails");

            migrationBuilder.DropColumn(
                name: "TCMPayStub_Filtro",
                table: "Settings");

            migrationBuilder.RenameColumn(
                name: "TCMPayStubId",
                table: "TCMPayStubDetails",
                newName: "BillId");

            migrationBuilder.RenameIndex(
                name: "IX_TCMPayStubDetails_TCMPayStubId",
                table: "TCMPayStubDetails",
                newName: "IX_TCMPayStubDetails_BillId");

            migrationBuilder.AddColumn<int>(
                name: "TCMPayStubEntityId",
                table: "BillDmsDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillDmsDetails_TCMPayStubEntityId",
                table: "BillDmsDetails",
                column: "TCMPayStubEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillDmsDetails_TCMPayStubs_TCMPayStubEntityId",
                table: "BillDmsDetails",
                column: "TCMPayStubEntityId",
                principalTable: "TCMPayStubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMPayStubDetails_TCMPayStubs_BillId",
                table: "TCMPayStubDetails",
                column: "BillId",
                principalTable: "TCMPayStubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
