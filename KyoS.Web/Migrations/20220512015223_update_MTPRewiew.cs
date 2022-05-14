using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class update_MTPRewiew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ACopy",
                table: "MTPReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateClinicalDirector",
                table: "MTPReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLicensedPractitioner",
                table: "MTPReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSignaturePerson",
                table: "MTPReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTherapist",
                table: "MTPReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DescribeAnyGoals",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeClient",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IfCurrent",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberUnit",
                table: "MTPReviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProviderNumber",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedOn",
                table: "MTPReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ServiceCode",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecifyChanges",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryOfServices",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TheConsumer",
                table: "MTPReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TheTreatmentPlan",
                table: "MTPReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ACopy",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "DateClinicalDirector",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "DateLicensedPractitioner",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "DateSignaturePerson",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "DateTherapist",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "DescribeAnyGoals",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "DescribeClient",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "IfCurrent",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "NumberUnit",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "ProviderNumber",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "ReviewedOn",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "ServiceCode",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "SpecifyChanges",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "SummaryOfServices",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "TheConsumer",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "TheTreatmentPlan",
                table: "MTPReviews");
        }
    }
}
