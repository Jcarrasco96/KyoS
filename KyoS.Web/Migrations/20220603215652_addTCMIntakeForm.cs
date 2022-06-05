using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMIntakeForm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCMIntakeForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    IntakeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResidentialStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyFamilyIncome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimarySourceIncome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TitlePosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Agency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsClientCurrently = table.Column<bool>(type: "bit", nullable: false),
                    Elibigility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MMA = table.Column<bool>(type: "bit", nullable: false),
                    LTC = table.Column<bool>(type: "bit", nullable: false),
                    School = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    School_Regular = table.Column<bool>(type: "bit", nullable: false),
                    School_ESE = table.Column<bool>(type: "bit", nullable: false),
                    School_EBD = table.Column<bool>(type: "bit", nullable: false),
                    School_ESOL = table.Column<bool>(type: "bit", nullable: false),
                    School_HHIP = table.Column<bool>(type: "bit", nullable: false),
                    School_Other = table.Column<bool>(type: "bit", nullable: false),
                    TeacherCounselor_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeacherCounselor_Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryContact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryContact_Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryContact_RelationShip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Other = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Other_Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Other_Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Other_City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NeedSpecial = table.Column<bool>(type: "bit", nullable: false),
                    NeedSpecial_Specify = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaseManagerNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeForms_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeForms_TcmClient_FK",
                table: "TCMIntakeForms",
                column: "TcmClient_FK",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeForms");
        }
    }
}
