using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class FacilitatorHumanResort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "Facilitators",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "Facilitators",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CAQH",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyEIN",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Facilitators",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CredentialExpirationDate",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Credentials",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DEALicense",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Facilitators",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialInstitutionsName",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Facilitators",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "HiringDate",
                table: "Facilitators",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "Facilitators",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicaidProviderID",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicareProviderID",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Money",
                table: "Facilitators",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NPI",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PH",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Facilitators",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Routing",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SSN",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Facilitators",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FacilitatorCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilitatorId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_FacilitatorCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilitatorCertifications_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FacilitatorCertifications_Facilitators_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalTable: "Facilitators",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacilitatorCertifications_CourseId",
                table: "FacilitatorCertifications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilitatorCertifications_FacilitatorId",
                table: "FacilitatorCertifications",
                column: "FacilitatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacilitatorCertifications");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "CAQH",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "CompanyEIN",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "CredentialExpirationDate",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "Credentials",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "DEALicense",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "FinancialInstitutionsName",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "HiringDate",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "MedicaidProviderID",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "MedicareProviderID",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "Money",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "NPI",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "PH",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "Routing",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "SSN",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Facilitators");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Facilitators");
        }
    }
}
