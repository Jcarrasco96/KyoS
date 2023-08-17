using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class PainScreenANDColumbiaSuicideInTCMintakeV11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfReferral",
                table: "TCMIntakePainScreen",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReferredTo",
                table: "TCMIntakePainScreen",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfReferral",
                table: "TCMIntakePainScreen");

            migrationBuilder.DropColumn(
                name: "ReferredTo",
                table: "TCMIntakePainScreen");
        }
    }
}
