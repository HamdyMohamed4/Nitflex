using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class j324424sdsdsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "Identity",
                table: "TVShowCasts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "Identity",
                table: "TVShowCasts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CurrentState",
                schema: "Identity",
                table: "TVShowCasts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "Identity",
                table: "TVShowCasts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "Identity",
                table: "TVShowCasts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Identity",
                table: "TVShowCasts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Identity",
                table: "TVShowCasts");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "Identity",
                table: "TVShowCasts");

            migrationBuilder.DropColumn(
                name: "CurrentState",
                schema: "Identity",
                table: "TVShowCasts");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "Identity",
                table: "TVShowCasts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "Identity",
                table: "TVShowCasts");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                schema: "Identity",
                table: "TVShowCasts");
        }
    }
}
