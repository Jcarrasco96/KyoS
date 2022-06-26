using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addAuditableEntityInTCM : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMSupervisors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMSupervisors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMStages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMStages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMStages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMStages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMServices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMServices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMServices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMServices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMServicePlans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMServicePlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMServicePlans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMServicePlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMServicePlanReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMServicePlanReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMServicePlanReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMServicePlanReviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMServicePlanReviewDomains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMServicePlanReviewDomains",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMServicePlanReviewDomains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMServicePlanReviewDomains",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMObjetives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMObjetives",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMObjetives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMObjetives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeWelcome",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeWelcome",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeWelcome",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeWelcome",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeOrientationCheckList",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeOrientationCheckList",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeOrientationCheckList",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeOrientationCheckList",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeNonClinicalLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeNonClinicalLog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeNonClinicalLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeNonClinicalLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeMiniMental",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeMiniMental",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeMiniMental",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeMiniMental",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeInterventionLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeInterventionLog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeInterventionLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeInterventionLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeIntervention",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeIntervention",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeIntervention",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeIntervention",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeForeignLanguage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeForeignLanguage",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeForeignLanguage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeForeignLanguage",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeCoordinationCare",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeCoordinationCare",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeCoordinationCare",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeCoordinationCare",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeConsumerRights",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeConsumerRights",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeConsumerRights",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeConsumerRights",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeConsentForTreatment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeConsentForTreatment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeConsentForTreatment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeConsentForTreatment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeConsentForRelease",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeConsentForRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeConsentForRelease",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeAppendixJ",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeAppendixJ",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeAppendixJ",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeAppendixJ",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeAdvancedDirective",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeAdvancedDirective",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeAdvancedDirective",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeAdvancedDirective",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeAcknowledgement",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeAcknowledgement",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeAcknowledgement",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeAcknowledgement",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMDomains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMDomains",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMDomains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMDomains",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMDischargeServiceStatus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMDischargeServiceStatus",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMDischargeServiceStatus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMDischargeServiceStatus",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMDischargeFollowUp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMDischargeFollowUp",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMDischargeFollowUp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMDischargeFollowUp",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMDischarge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMDischarge",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMDischarge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMDischarge",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMClient",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMClient",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMClient",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMClient",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMAdendums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMAdendums",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMAdendums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMAdendums",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMStages");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMStages");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMStages");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMStages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMServices");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMServices");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMServices");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMServices");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMServicePlanReviews");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMServicePlanReviewDomains");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMServicePlanReviewDomainObjectives");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMObjetives");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMObjetives");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMObjetives");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMObjetives");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeWelcome");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeWelcome");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeWelcome");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeWelcome");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeOrientationCheckList");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeOrientationCheckList");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeOrientationCheckList");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeOrientationCheckList");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeNonClinicalLog");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeNonClinicalLog");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeNonClinicalLog");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeNonClinicalLog");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeMiniMental");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeMiniMental");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeMiniMental");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeMiniMental");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeInterventionLog");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeInterventionLog");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeInterventionLog");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeInterventionLog");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeIntervention");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeIntervention");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeIntervention");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeIntervention");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeForeignLanguage");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeForeignLanguage");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeForeignLanguage");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeForeignLanguage");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeCoordinationCare");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeCoordinationCare");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeCoordinationCare");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeCoordinationCare");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeConsumerRights");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeConsumerRights");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeConsumerRights");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeConsumerRights");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeConsentForTreatment");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeConsentForTreatment");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeConsentForTreatment");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeConsentForTreatment");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeAdvancedDirective");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeAdvancedDirective");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeAdvancedDirective");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeAdvancedDirective");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeAcknowledgement");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeAcknowledgement");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeAcknowledgement");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeAcknowledgement");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMDomains");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMDomains");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMDomains");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMDomains");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMDischargeServiceStatus");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMDischargeServiceStatus");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMDischargeServiceStatus");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMDischargeServiceStatus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMDischargeFollowUp");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMDischargeFollowUp");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMDischargeFollowUp");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMDischargeFollowUp");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMDischarge");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMDischarge");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMDischarge");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMDischarge");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMAdendums");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMAdendums");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMAdendums");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMAdendums");
        }
    }
}
