using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class DocumentAssistantHumanResort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "DocumentsAssistant",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "DocumentsAssistant",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CAQH",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyEIN",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "DocumentsAssistant",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CredentialExpirationDate",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Credentials",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DEALicense",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "DocumentsAssistant",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialInstitutionsName",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "DocumentsAssistant",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "HiringDate",
                table: "DocumentsAssistant",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "DocumentsAssistant",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicaidProviderID",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicareProviderID",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Money",
                table: "DocumentsAssistant",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NPI",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PH",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "DocumentsAssistant",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Routing",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SSN",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DocumentsAssistant",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "DocumentsAssistant",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentAssistantCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentAssistantId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_DocumentAssistantCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentAssistantCertifications_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentAssistantCertifications_DocumentsAssistant_DocumentAssistantId",
                        column: x => x.DocumentAssistantId,
                        principalTable: "DocumentsAssistant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAssistantCertifications_CourseId",
                table: "DocumentAssistantCertifications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAssistantCertifications_DocumentAssistantId",
                table: "DocumentAssistantCertifications",
                column: "DocumentAssistantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentAssistantCertifications");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "CAQH",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "City",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "CompanyEIN",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "CredentialExpirationDate",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "Credentials",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "DEALicense",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "FinancialInstitutionsName",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "HiringDate",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "MedicaidProviderID",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "MedicareProviderID",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "Money",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "NPI",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "PH",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "Routing",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "SSN",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "State",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DocumentsAssistant");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "DocumentsAssistant");
        }
    }
}
