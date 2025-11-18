using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class m5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDevices",
                schema: "Identity",
                table: "SubscriptionPlans");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "Identity",
                table: "SubscriptionPlans",
                newName: "SupportedDevices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupportedDevices",
                schema: "Identity",
                table: "SubscriptionPlans",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "MaxDevices",
                schema: "Identity",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
