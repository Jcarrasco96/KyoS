using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AppendixJmoreOneForClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMIntakeAppendixJ_TCMClient_TcmClient_FK",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropIndex(
                name: "IX_TCMIntakeAppendixJ_TcmClient_FK",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateExpired",
                table: "TCMIntakeAppendixJ",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TcmClientId",
                table: "TCMIntakeAppendixJ",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeAppendixJ_TcmClientId",
                table: "TCMIntakeAppendixJ",
                column: "TcmClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMIntakeAppendixJ_TCMClient_TcmClientId",
                table: "TCMIntakeAppendixJ",
                column: "TcmClientId",
                principalTable: "TCMClient",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMIntakeAppendixJ_TCMClient_TcmClientId",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropIndex(
                name: "IX_TCMIntakeAppendixJ_TcmClientId",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "DateExpired",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "TcmClientId",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeAppendixJ_TcmClient_FK",
                table: "TCMIntakeAppendixJ",
                column: "TcmClient_FK",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMIntakeAppendixJ_TCMClient_TcmClient_FK",
                table: "TCMIntakeAppendixJ",
                column: "TcmClient_FK",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
