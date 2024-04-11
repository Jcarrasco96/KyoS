using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class improvesCaseManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "CaseManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "CaseManagers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CAQH",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyEIN",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "CaseManagers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CredentialExpirationDate",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DEALicense",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "CaseManagers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FinancialInstitutionsName",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "CaseManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "HiringDate",
                table: "CaseManagers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "CaseManagers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicaidProviderID",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicareProviderID",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NPI",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PH",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "CaseManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Routing",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SSN",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "CaseManagers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "CAQH",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "CompanyEIN",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "CredentialExpirationDate",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "DEALicense",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "FinancialInstitutionsName",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "HiringDate",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "MedicaidProviderID",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "MedicareProviderID",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "NPI",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "PH",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "Routing",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "SSN",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "State",
                table: "CaseManagers");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "CaseManagers");
        }
    }
}
