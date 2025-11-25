using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class j324424sd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BannerUrl",
                schema: "Identity",
                table: "TVShows",
                newName: "VideoUrl");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                schema: "Identity",
                table: "TVShows",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TmdbId",
                schema: "Identity",
                table: "TVShows",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioType",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                schema: "Identity",
                table: "TVShows",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                schema: "Identity",
                table: "TVShows",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReleaseYear",
                schema: "Identity",
                table: "TVShows",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrailerUrl",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioType",
                schema: "Identity",
                table: "TVShows");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                schema: "Identity",
                table: "TVShows");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                schema: "Identity",
                table: "TVShows");

            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                schema: "Identity",
                table: "TVShows");

            migrationBuilder.DropColumn(
                name: "TrailerUrl",
                schema: "Identity",
                table: "TVShows");

            migrationBuilder.RenameColumn(
                name: "VideoUrl",
                schema: "Identity",
                table: "TVShows",
                newName: "BannerUrl");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TmdbId",
                schema: "Identity",
                table: "TVShows",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                schema: "Identity",
                table: "TVShows",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
