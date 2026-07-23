using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigiPOSE.Migrations
{
    /// <inheritdoc />
    public partial class Convert_To_Explicit_PermissionRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionRole");

            migrationBuilder.CreateTable(
                name: "PermissionRoles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionRoles", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_PermissionRoles_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermissionRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PermissionRoles",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 11, 1 },
                    { 12, 1 },
                    { 13, 1 },
                    { 14, 1 },
                    { 15, 1 },
                    { 16, 1 },
                    { 17, 1 },
                    { 18, 1 },
                    { 19, 1 },
                    { 20, 1 },
                    { 5, 2 },
                    { 6, 2 },
                    { 7, 2 },
                    { 8, 2 },
                    { 9, 2 },
                    { 10, 2 },
                    { 12, 2 },
                    { 13, 2 },
                    { 18, 2 },
                    { 19, 2 },
                    { 5, 3 },
                    { 6, 3 },
                    { 7, 3 },
                    { 10, 3 },
                    { 10, 4 },
                    { 11, 4 },
                    { 13, 4 },
                    { 14, 4 },
                    { 14, 5 },
                    { 15, 5 },
                    { 16, 5 },
                    { 17, 5 },
                    { 18, 6 },
                    { 19, 6 },
                    { 20, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRoles_PermissionId",
                table: "PermissionRoles",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionRoles");

            migrationBuilder.CreateTable(
                name: "PermissionRole",
                columns: table => new
                {
                    PermissionsPermissionId = table.Column<int>(type: "int", nullable: false),
                    RolesRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionRole", x => new { x.PermissionsPermissionId, x.RolesRoleId });
                    table.ForeignKey(
                        name: "FK_PermissionRole_Permissions_PermissionsPermissionId",
                        column: x => x.PermissionsPermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermissionRole_Roles_RolesRoleId",
                        column: x => x.RolesRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PermissionRole",
                columns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 5, 2 },
                    { 5, 3 },
                    { 6, 1 },
                    { 6, 2 },
                    { 6, 3 },
                    { 7, 1 },
                    { 7, 2 },
                    { 7, 3 },
                    { 8, 1 },
                    { 8, 2 },
                    { 9, 1 },
                    { 9, 2 },
                    { 10, 1 },
                    { 10, 2 },
                    { 10, 3 },
                    { 10, 4 },
                    { 11, 1 },
                    { 11, 4 },
                    { 12, 1 },
                    { 12, 2 },
                    { 13, 1 },
                    { 13, 2 },
                    { 13, 4 },
                    { 14, 1 },
                    { 14, 4 },
                    { 14, 5 },
                    { 15, 1 },
                    { 15, 5 },
                    { 16, 1 },
                    { 16, 5 },
                    { 17, 1 },
                    { 17, 5 },
                    { 18, 1 },
                    { 18, 2 },
                    { 18, 6 },
                    { 19, 1 },
                    { 19, 2 },
                    { 19, 6 },
                    { 20, 1 },
                    { 20, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRole_RolesRoleId",
                table: "PermissionRole",
                column: "RolesRoleId");
        }
    }
}
