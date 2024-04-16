using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class TCMSupervisorCertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "TCMSupervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "TCMSupervisors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CAQH",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyEIN",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CredentialExpirationDate",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DEALicense",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "TCMSupervisors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FinancialInstitutionsName",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "TCMSupervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "HiringDate",
                table: "TCMSupervisors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicaidProviderID",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicareProviderID",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NPI",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PH",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "TCMSupervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Routing",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SSN",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "TCMSupervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TCMSupervisorCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCMSupervisorId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_TCMSupervisorCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMSupervisorCertifications_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TCMSupervisorCertifications_TCMSupervisors_TCMSupervisorId",
                        column: x => x.TCMSupervisorId,
                        principalTable: "TCMSupervisors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMSupervisorCertifications_CourseId",
                table: "TCMSupervisorCertifications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMSupervisorCertifications_TCMSupervisorId",
                table: "TCMSupervisorCertifications",
                column: "TCMSupervisorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMSupervisorCertifications");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "CAQH",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "City",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "CompanyEIN",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "CredentialExpirationDate",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "DEALicense",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "FinancialInstitutionsName",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "HiringDate",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "MedicaidProviderID",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "MedicareProviderID",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "NPI",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "PH",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "Routing",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "SSN",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "State",
                table: "TCMSupervisors");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "TCMSupervisors");
        }
    }
}
