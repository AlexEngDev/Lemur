using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lemur.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Permissions");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Groups");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
