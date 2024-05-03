using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class SupervisorHumanResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "Supervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "Supervisors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CAQH",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyEIN",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Supervisors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CredentialExpirationDate",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Credentials",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DEALicense",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Supervisors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialInstitutionsName",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Supervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "HiringDate",
                table: "Supervisors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "Supervisors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicaidProviderID",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicareProviderID",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Money",
                table: "Supervisors",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NPI",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PH",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Supervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Routing",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SSN",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupervisorCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    CourseId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CertificationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupervisorCertifications_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupervisorCertifications_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorCertifications_CourseId",
                table: "SupervisorCertifications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorCertifications_SupervisorId",
                table: "SupervisorCertifications",
                column: "SupervisorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupervisorCertifications");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "CAQH",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "CompanyEIN",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "CredentialExpirationDate",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Credentials",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "DEALicense",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "FinancialInstitutionsName",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "HiringDate",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "MedicaidProviderID",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "MedicareProviderID",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Money",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "NPI",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "PH",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "Routing",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "SSN",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Supervisors");
        }
    }
}
