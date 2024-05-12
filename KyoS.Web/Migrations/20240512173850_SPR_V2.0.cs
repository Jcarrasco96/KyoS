using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class SPR_V20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChangesUpdate",
                table: "TCMServicePlanReviewDomainObjectives",
                newName: "Task");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAssessment",
                table: "TCMServicePlanReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCertification",
                table: "TCMServicePlanReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAccomplished",
                table: "TCMServicePlanReviewDomains",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateIdentified",
                table: "TCMServicePlanReviewDomains",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LongTerm",
                table: "TCMServicePlanReviewDomains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TCMServicePlanReviewDomains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NeedsIdentified",
                table: "TCMServicePlanReviewDomains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Finish",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Responsible",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetDate",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateAssessment",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "DateCertification",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "DateAccomplished",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "DateIdentified",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "LongTerm",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "NeedsIdentified",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "Finish",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.DropColumn(
                name: "Responsible",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.DropColumn(
                name: "TargetDate",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.RenameColumn(
                name: "Task",
                table: "TCMServicePlanReviewDomainObjectives",
                newName: "ChangesUpdate");
        }
    }
}
