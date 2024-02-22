using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class facilitatorInMtpR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FacilitatorId",
                table: "MTPReviews",
                type: "int",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MTPReviews_Facilitators_FacilitatorId",
                table: "MTPReviews");

            migrationBuilder.DropIndex(
                name: "IX_MTPReviews_FacilitatorId",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "FacilitatorId",
                table: "MTPReviews");
        }
    }
}
