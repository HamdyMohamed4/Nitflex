using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class GG : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "Identity",
                table: "MovieGenres",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "Identity",
                table: "MovieGenres",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CurrentState",
                schema: "Identity",
                table: "MovieGenres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "Identity",
                table: "MovieGenres",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "Identity",
                table: "MovieGenres",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                schema: "Identity",
                table: "MovieGenres",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Identity",
                table: "MovieGenres");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "Identity",
                table: "MovieGenres");

            migrationBuilder.DropColumn(
                name: "CurrentState",
                schema: "Identity",
                table: "MovieGenres");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "Identity",
                table: "MovieGenres");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "Identity",
                table: "MovieGenres");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                schema: "Identity",
                table: "MovieGenres");
        }
    }
}
