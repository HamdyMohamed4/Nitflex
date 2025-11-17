using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class updateSubbscriptionplan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxDownloadDevices",
                schema: "Identity",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxSimultaneousDevices",
                schema: "Identity",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SpatialAudio",
                schema: "Identity",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoAndSoundQuality",
                schema: "Identity",
                table: "SubscriptionPlans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDownloadDevices",
                schema: "Identity",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxSimultaneousDevices",
                schema: "Identity",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "SpatialAudio",
                schema: "Identity",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "VideoAndSoundQuality",
                schema: "Identity",
                table: "SubscriptionPlans");
        }
    }
}
