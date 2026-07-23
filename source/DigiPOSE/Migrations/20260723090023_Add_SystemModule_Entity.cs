using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigiPOSE.Migrations
{
    /// <inheritdoc />
    public partial class Add_SystemModule_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SystemModule",
                table: "Permissions");

            migrationBuilder.AddColumn<int>(
                name: "SystemModuleId",
                table: "Permissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemModules",
                columns: table => new
                {
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemModules", x => x.ModuleId);
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "SystemModuleId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "SystemModuleId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "SystemModuleId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "SystemModuleId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "SystemModuleId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "SystemModuleId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "SystemModuleId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "SystemModuleId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "SystemModuleId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "SystemModuleId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "SystemModuleId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 12,
                column: "SystemModuleId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 13,
                column: "SystemModuleId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 14,
                column: "SystemModuleId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 15,
                column: "SystemModuleId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 16,
                column: "SystemModuleId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 17,
                column: "SystemModuleId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 18,
                column: "SystemModuleId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 19,
                column: "SystemModuleId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 20,
                column: "SystemModuleId",
                value: 5);

            migrationBuilder.InsertData(
                table: "SystemModules",
                columns: new[] { "ModuleId", "Icon", "IsActive", "ModuleName", "SortOrder" },
                values: new object[,]
                {
                    { 1, "fa-solid fa-cogs", true, "System", 1 },
                    { 2, "fa-solid fa-cash-register", true, "POS", 2 },
                    { 3, "fa-solid fa-warehouse", true, "Warehouse", 3 },
                    { 4, "fa-solid fa-tags", true, "Catalog", 4 },
                    { 5, "fa-solid fa-file-invoice-dollar", true, "Finance", 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_SystemModuleId",
                table: "Permissions",
                column: "SystemModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_SystemModules_SystemModuleId",
                table: "Permissions",
                column: "SystemModuleId",
                principalTable: "SystemModules",
                principalColumn: "ModuleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_SystemModules_SystemModuleId",
                table: "Permissions");

            migrationBuilder.DropTable(
                name: "SystemModules");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_SystemModuleId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "SystemModuleId",
                table: "Permissions");

            migrationBuilder.AddColumn<string>(
                name: "SystemModule",
                table: "Permissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1,
                column: "SystemModule",
                value: "System");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2,
                column: "SystemModule",
                value: "System");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3,
                column: "SystemModule",
                value: "System");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4,
                column: "SystemModule",
                value: "System");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5,
                column: "SystemModule",
                value: "POS");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6,
                column: "SystemModule",
                value: "POS");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7,
                column: "SystemModule",
                value: "POS");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8,
                column: "SystemModule",
                value: "POS");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9,
                column: "SystemModule",
                value: "POS");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10,
                column: "SystemModule",
                value: "Warehouse");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11,
                column: "SystemModule",
                value: "Warehouse");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 12,
                column: "SystemModule",
                value: "Warehouse");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 13,
                column: "SystemModule",
                value: "Warehouse");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 14,
                column: "SystemModule",
                value: "Warehouse");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 15,
                column: "SystemModule",
                value: "Catalog");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 16,
                column: "SystemModule",
                value: "Catalog");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 17,
                column: "SystemModule",
                value: "Catalog");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 18,
                column: "SystemModule",
                value: "Finance");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 19,
                column: "SystemModule",
                value: "Finance");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 20,
                column: "SystemModule",
                value: "Finance");
        }
    }
}
