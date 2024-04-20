using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class ImprovesAuthorizationV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "HealthInsuranceTemp",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndCoverageDate",
                table: "HealthInsuranceTemp",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "InsurancePlan",
                table: "HealthInsuranceTemp",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InsuranceType",
                table: "HealthInsuranceTemp",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "Clients_HealthInsurances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndCoverageDate",
                table: "Clients_HealthInsurances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "InsurancePlan",
                table: "Clients_HealthInsurances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InsuranceType",
                table: "Clients_HealthInsurances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "HealthInsuranceTemp");

            migrationBuilder.DropColumn(
                name: "EndCoverageDate",
                table: "HealthInsuranceTemp");

            migrationBuilder.DropColumn(
                name: "InsurancePlan",
                table: "HealthInsuranceTemp");

            migrationBuilder.DropColumn(
                name: "InsuranceType",
                table: "HealthInsuranceTemp");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "Clients_HealthInsurances");

            migrationBuilder.DropColumn(
                name: "EndCoverageDate",
                table: "Clients_HealthInsurances");

            migrationBuilder.DropColumn(
                name: "InsurancePlan",
                table: "Clients_HealthInsurances");

            migrationBuilder.DropColumn(
                name: "InsuranceType",
                table: "Clients_HealthInsurances");
        }
    }
}
