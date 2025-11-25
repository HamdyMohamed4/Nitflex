using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class j324424sdsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrailerUrl",
                schema: "Identity",
                table: "TVShows");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                schema: "Identity",
                table: "TVShows");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrailerUrl",
                schema: "Identity",
                table: "Episodes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                schema: "Identity",
                table: "TVShows");

            migrationBuilder.DropColumn(
                name: "TrailerUrl",
                schema: "Identity",
                table: "Episodes");

            migrationBuilder.AddColumn<string>(
                name: "TrailerUrl",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
