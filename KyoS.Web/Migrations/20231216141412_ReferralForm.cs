using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ReferralForm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Supervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ReferralForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_FK = table.Column<int>(type: "int", nullable: false),
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
                    CaseAcceptedFacilitator = table.Column<bool>(type: "bit", nullable: false),
                    CaseAcceptedSupervisor = table.Column<bool>(type: "bit", nullable: false),
                    DateAssigned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FacilitatorSign = table.Column<bool>(type: "bit", nullable: false),
                    SupervisorSign = table.Column<bool>(type: "bit", nullable: false),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    FacilitatorId = table.Column<int>(type: "int", nullable: true),
                    AssignedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralForms_Clients_Client_FK",
                        column: x => x.Client_FK,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReferralForms_Facilitators_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalTable: "Facilitators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralForms_Supervisors_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Supervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferralForms_Client_FK",
                table: "ReferralForms",
                column: "Client_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralForms_FacilitatorId",
                table: "ReferralForms",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralForms_SupervisorId",
                table: "ReferralForms",
                column: "SupervisorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferralForms");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Supervisors");
        }
    }
}
