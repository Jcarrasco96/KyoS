using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class MTPRfacilitatorIdChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MTPReviews_Facilitators_FacilitatorId",
                table: "MTPReviews");

            migrationBuilder.DropIndex(
                name: "IX_MTPReviews_FacilitatorId",
                table: "MTPReviews");

            migrationBuilder.AlterColumn<int>(
                name: "FacilitatorId",
                table: "MTPReviews",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FacilitatorId",
                table: "MTPReviews",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_MTPReviews_FacilitatorId",
                table: "MTPReviews",
                column: "FacilitatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_MTPReviews_Facilitators_FacilitatorId",
                table: "MTPReviews",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id");
        }
    }
}
