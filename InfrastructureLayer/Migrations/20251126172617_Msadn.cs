using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class Msadn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsKidProfile",
                schema: "Identity",
                table: "Profiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                schema: "Identity",
                table: "Profiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PinHash",
                schema: "Identity",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsKidProfile",
                schema: "Identity",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                schema: "Identity",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "PinHash",
                schema: "Identity",
                table: "Profiles");
        }
    }
}
