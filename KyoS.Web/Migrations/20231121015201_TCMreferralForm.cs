using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMreferralForm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMReferralForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    NameClient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SSN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalGuardianName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalGuardianPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dx = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dx_Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferredBy_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferredBy_Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferredBy_Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicaidId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HMO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitsApproved = table.Column<int>(type: "int", nullable: false),
                    AuthorizedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExperatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameSupervisor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaseAccepted = table.Column<bool>(type: "bit", nullable: false),
                    DateAssigned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TCMSign = table.Column<bool>(type: "bit", nullable: false),
                    TCMSupervisorSign = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMReferralForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMReferralForms_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMReferralForms_TcmClient_FK",
                table: "TCMReferralForms",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMReferralForms");
        }
    }
}
