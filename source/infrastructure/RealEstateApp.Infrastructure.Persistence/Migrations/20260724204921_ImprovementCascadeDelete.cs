using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImprovementCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImprovements_Improvements_ImprovementId",
                table: "PropertyImprovements");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyImprovements_Improvements_ImprovementId",
                table: "PropertyImprovements",
                column: "ImprovementId",
                principalTable: "Improvements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImprovements_Improvements_ImprovementId",
                table: "PropertyImprovements");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyImprovements_Improvements_ImprovementId",
                table: "PropertyImprovements",
                column: "ImprovementId",
                principalTable: "Improvements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
