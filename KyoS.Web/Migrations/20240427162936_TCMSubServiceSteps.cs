using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class TCMSubServiceSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Frecuency",
                table: "TCMSubServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "TCMSubServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TCMSubServiceSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    TcmSubServiceId = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Units = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMSubServiceSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMSubServiceSteps_TCMSubServices_TcmSubServiceId",
                        column: x => x.TcmSubServiceId,
                        principalTable: "TCMSubServices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMSubServiceSteps_TcmSubServiceId",
                table: "TCMSubServiceSteps",
                column: "TcmSubServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMSubServiceSteps");

            migrationBuilder.DropColumn(
                name: "Frecuency",
                table: "TCMSubServices");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "TCMSubServices");
        }
    }
}
