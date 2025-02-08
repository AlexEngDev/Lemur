using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lemur.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMoltiAMolti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupPermission_Permissions_CommandId",
                table: "GroupPermission");

            migrationBuilder.RenameColumn(
                name: "CommandId",
                table: "GroupPermission",
                newName: "PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupPermission_CommandId",
                table: "GroupPermission",
                newName: "IX_GroupPermission_PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPermission_Permissions_PermissionId",
                table: "GroupPermission",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupPermission_Permissions_PermissionId",
                table: "GroupPermission");

            migrationBuilder.RenameColumn(
                name: "PermissionId",
                table: "GroupPermission",
                newName: "CommandId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupPermission_PermissionId",
                table: "GroupPermission",
                newName: "IX_GroupPermission_CommandId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPermission_Permissions_CommandId",
                table: "GroupPermission",
                column: "CommandId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
