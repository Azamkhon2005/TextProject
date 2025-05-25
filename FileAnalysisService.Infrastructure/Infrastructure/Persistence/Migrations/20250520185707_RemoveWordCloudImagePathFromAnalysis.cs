using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileAnalysisService.Infrastructure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWordCloudImagePathFromAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WordCloudImagePath",
                table: "AnalysisResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WordCloudImagePath",
                table: "AnalysisResults",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }
    }
}
