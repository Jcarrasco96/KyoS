using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateTCM_SPR : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClientContinue",
                table: "TCMServicePlanReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClientHasBeen1",
                table: "TCMServicePlanReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClientHasBeen2",
                table: "TCMServicePlanReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClientNoLonger1",
                table: "TCMServicePlanReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClientNoLonger2",
                table: "TCMServicePlanReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClientWillContinue",
                table: "TCMServicePlanReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClientWillHave",
                table: "TCMServicePlanReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTCMCaseManagerSignature",
                table: "TCMServicePlanReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTCMSupervisorSignature",
                table: "TCMServicePlanReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "HasBeenExplained",
                table: "TCMServicePlanReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TheExpertedReviewDate",
                table: "TCMServicePlanReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientContinue",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "ClientHasBeen1",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "ClientHasBeen2",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "ClientNoLonger1",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "ClientNoLonger2",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "ClientWillContinue",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "ClientWillHave",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "DateTCMCaseManagerSignature",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "DateTCMSupervisorSignature",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "HasBeenExplained",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "TheExpertedReviewDate",
                table: "TCMServicePlanReviews");
        }
    }
}
