using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerLab.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEnableStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PluginRegistries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PluginRegistries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
