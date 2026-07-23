using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigiPOSE.Migrations
{
    /// <inheritdoc />
    public partial class Add_Seed_Data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "Description", "ImageUrl", "IsActive", "Slug" },
                values: new object[,]
                {
                    { 1, "Electronics", "Electronic devices and accessories", null, true, "electronics" },
                    { 2, "Home Appliances", "Appliances for home use", null, true, "home-appliances" }
                });

            migrationBuilder.InsertData(
                table: "CustomerTypes",
                columns: new[] { "CustomeTypeId", "Description", "TypeName" },
                values: new object[,]
                {
                    { 1, "Standard one-time buyers", "Walk-in Customer" },
                    { 2, "Loyal customers with membership benefits", "VIP Member" }
                });

            migrationBuilder.InsertData(
                table: "InvoiceStatuses",
                columns: new[] { "InvoiceStatusId", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, "Fully paid invoice", "Paid" },
                    { 2, "Payment pending", "Unpaid" },
                    { 3, "Canceled invoice", "Void" }
                });

            migrationBuilder.InsertData(
                table: "InvoiceTypes",
                columns: new[] { "InvoiceTypeId", "Description", "TypeName" },
                values: new object[,]
                {
                    { 1, "Standard POS checkout", "Standard Sale" },
                    { 2, "Customer return invoice", "Return" }
                });

            migrationBuilder.InsertData(
                table: "ItemNatures",
                columns: new[] { "NatureId", "IsActive", "NatureName", "TaxXmlCode" },
                values: new object[,]
                {
                    { 1, true, "Physical Goods", "1" },
                    { 2, true, "Digital Service", "2" }
                });

            migrationBuilder.InsertData(
                table: "Manufacturers",
                columns: new[] { "ManufacturerId", "Address", "ContactPerson", "Description", "Email", "ImageUrl", "IsActive", "ManufacturerName", "Phone", "Slug", "TaxCode", "Website" },
                values: new object[,]
                {
                    { 1, null, null, "Sony Electronics Corporation", null, null, true, "Sony", null, "sony", null, null },
                    { 2, null, null, "Samsung Electronics", null, null, true, "Samsung", null, "samsung", null, null }
                });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "StatusId", "BadgeColor", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, null, "Order finalized and delivered", "Completed" },
                    { 2, null, "Order awaiting payment", "Pending" }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "PaymentMethodId", "Description", "MethodName" },
                values: new object[,]
                {
                    { 1, "Cash payment at counter", "Cash" },
                    { 2, "Visa/Mastercard", "Credit Card" },
                    { 3, "Momo, ZaloPay, etc.", "E-Wallet" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Description", "PermissionName", "SystemModule" },
                values: new object[,]
                {
                    { 1, "Manage global system configurations and settings.", "System.Config.Manage", "System" },
                    { 2, "Create, edit, and delete branches and counters.", "System.Branch.Manage", "System" },
                    { 3, "Create, edit, and delete user roles and permissions.", "System.Role.Manage", "System" },
                    { 4, "Manage user accounts, reset passwords, and assign roles.", "System.User.Manage", "System" },
                    { 5, "Open a new cashier shift and declare starting cash.", "POS.Shift.Open", "POS" },
                    { 6, "Close an active shift and declare ending cash.", "POS.Shift.Close", "POS" },
                    { 7, "Create new retail orders and process payments.", "POS.Order.Create", "POS" },
                    { 8, "Cancel or void a completed order/invoice.", "POS.Order.Void", "POS" },
                    { 9, "Apply special discounts beyond standard limits.", "POS.Discount.Apply", "POS" },
                    { 10, "View current product stock levels.", "Warehouse.Inventory.View", "Warehouse" },
                    { 11, "Create stock-in, stock-out, or transfer vouchers.", "Warehouse.Voucher.Create", "Warehouse" },
                    { 12, "Approve pending stock vouchers for processing.", "Warehouse.Voucher.Approve", "Warehouse" },
                    { 13, "Adjust stock quantities after physical counting.", "Warehouse.Inventory.Adjust", "Warehouse" },
                    { 14, "Manage supplier profiles and information.", "Warehouse.Supplier.Manage", "Warehouse" },
                    { 15, "Create, edit, or disable product records.", "Catalog.Product.Manage", "Catalog" },
                    { 16, "Manage product categories, types, and units.", "Catalog.Category.Manage", "Catalog" },
                    { 17, "Update base prices and tax settings for products.", "Catalog.Price.Manage", "Catalog" },
                    { 18, "View financial dashboards and revenue reports.", "Finance.Report.View", "Finance" },
                    { 19, "View and search all historical invoices and receipts.", "Finance.Invoice.View", "Finance" },
                    { 20, "Export financial data and audit logs for tax purposes.", "Finance.Audit.Export", "Finance" }
                });

            migrationBuilder.InsertData(
                table: "ProductTypes",
                columns: new[] { "ProductTypeId", "IsActive", "IsInventoryTracked", "TypeName" },
                values: new object[,]
                {
                    { 1, true, true, "Standard Product" },
                    { 2, true, false, "Bundle" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Description", "RoleName" },
                values: new object[,]
                {
                    { 1, "The person with the highest authority, managing the overall data infrastructure.", "Super Admin" },
                    { 2, "Responsible for operations at a specific branch.", "Branch Manager" },
                    { 3, "The person directly at the counter to handle sales transactions.", "POS Operator" },
                    { 4, "Responsible for managing goods, receiving/shipping/stocking.", "Warehouse" },
                    { 5, "Personnel responsible for maintaining product information.", "Catalog" },
                    { 6, "Controls cash flow, tax reporting, and reconciles data.", "Accountant" }
                });

            migrationBuilder.InsertData(
                table: "ShiftStatuses",
                columns: new[] { "StatusId", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, "Shift is currently open", "Active" },
                    { 2, "Shift has ended and cash declared", "Closed" }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "SupplierId", "Address", "BankAccount", "BankName", "ContactPerson", "DebtBalance", "Description", "Email", "ImageUrl", "IsActive", "Phone", "SupplierName", "TaxCode", "Website" },
                values: new object[,]
                {
                    { 1, null, null, null, null, 0m, null, "contact@globalsupply.com", null, true, null, "Global Supply Co.", null, null },
                    { 2, null, null, null, null, 0m, null, "sales@techdist.com", null, true, null, "Tech Distributors", null, null }
                });

            migrationBuilder.InsertData(
                table: "TaxTypes",
                columns: new[] { "TaxTypeId", "IsActive", "TaxName", "TaxPercentage", "TaxXmlCode" },
                values: new object[,]
                {
                    { 1, true, "VAT 10%", 10m, "10" },
                    { 2, true, "VAT 8%", 8m, "8" },
                    { 3, true, "Tax Free", 0m, "0" }
                });

            migrationBuilder.InsertData(
                table: "Units",
                columns: new[] { "UnitId", "Description", "UnitName" },
                values: new object[,]
                {
                    { 1, "Single unit", "Piece" },
                    { 2, "Packaged box", "Box" },
                    { 3, "Kilogram", "Kg" }
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CustomerTypes",
                keyColumn: "CustomeTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CustomerTypes",
                keyColumn: "CustomeTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InvoiceStatuses",
                keyColumn: "InvoiceStatusId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InvoiceStatuses",
                keyColumn: "InvoiceStatusId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InvoiceStatuses",
                keyColumn: "InvoiceStatusId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "InvoiceTypes",
                keyColumn: "InvoiceTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InvoiceTypes",
                keyColumn: "InvoiceTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ItemNatures",
                keyColumn: "NatureId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ItemNatures",
                keyColumn: "NatureId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Manufacturers",
                keyColumn: "ManufacturerId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Manufacturers",
                keyColumn: "ManufacturerId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "StatusId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "StatusId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PaymentMethods",
                keyColumn: "PaymentMethodId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PaymentMethods",
                keyColumn: "PaymentMethodId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PaymentMethods",
                keyColumn: "PaymentMethodId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 5, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 5, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 5, 3 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 6, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 6, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 6, 3 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 7, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 7, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 7, 3 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 8, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 8, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 9, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 9, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 10, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 10, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 10, 3 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 10, 4 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 11, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 11, 4 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 12, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 12, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 13, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 13, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 13, 4 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 14, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 14, 4 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 14, 5 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 15, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 15, 5 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 16, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 16, 5 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 17, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 17, 5 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 18, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 18, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 18, 6 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 19, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 19, 2 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 19, 6 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 20, 1 });

            migrationBuilder.DeleteData(
                table: "PermissionRole",
                keyColumns: new[] { "PermissionsPermissionId", "RolesRoleId" },
                keyValues: new object[] { 20, 6 });

            migrationBuilder.DeleteData(
                table: "ProductTypes",
                keyColumn: "ProductTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProductTypes",
                keyColumn: "ProductTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ShiftStatuses",
                keyColumn: "StatusId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ShiftStatuses",
                keyColumn: "StatusId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "SupplierId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "SupplierId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TaxTypes",
                keyColumn: "TaxTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TaxTypes",
                keyColumn: "TaxTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TaxTypes",
                keyColumn: "TaxTypeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 6);
        }
    }
}
