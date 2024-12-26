using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class rolesSeedingII : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019a5671-bd23-4d74-8c4e-8a3f9596024d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "13e2ccc5-5a0b-427a-9c91-66542b3b4d85");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3b93dba7-7184-49fe-9f55-d7a80b3b2b19");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "647cb9b9-f9e9-4dcf-ba6b-259115d4a5b1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "65b30edc-edb0-4daa-bb00-0b7110fa8bb8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ba120fdf-05db-48b7-bb25-d78003eaf1ca");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f227fff3-37ea-413d-af37-27e1ce3a64ba");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0f653b58-2315-43ed-9d55-fc4c73dc75f1", "7", "BD", "BD" },
                    { "36211113-2c97-4a54-9de0-2c6beb7bcc43", "6", "Access", "Access" },
                    { "51875afb-56b0-4545-adcb-9de6a7770c85", "5", "Employee", "Employee" },
                    { "5c408a1e-7f20-47a2-a450-e9be89fa1513", "2", "HR", "HR" },
                    { "7629a37f-eb5a-4ac9-9b18-c7d0555857ed", "4", "Stores", "Stores" },
                    { "ddf44980-5a84-49e6-b91b-150683934d44", "3", "Sales", "Sales" },
                    { "ec99cad0-acdf-4dfb-a715-8a9632610423", "1", "Admin", "Admin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0f653b58-2315-43ed-9d55-fc4c73dc75f1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "36211113-2c97-4a54-9de0-2c6beb7bcc43");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "51875afb-56b0-4545-adcb-9de6a7770c85");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5c408a1e-7f20-47a2-a450-e9be89fa1513");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7629a37f-eb5a-4ac9-9b18-c7d0555857ed");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ddf44980-5a84-49e6-b91b-150683934d44");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ec99cad0-acdf-4dfb-a715-8a9632610423");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "019a5671-bd23-4d74-8c4e-8a3f9596024d", "6", "Access", "Access" },
                    { "13e2ccc5-5a0b-427a-9c91-66542b3b4d85", "4", "Stores", "Stores" },
                    { "3b93dba7-7184-49fe-9f55-d7a80b3b2b19", "1", "Admin", "Admin" },
                    { "647cb9b9-f9e9-4dcf-ba6b-259115d4a5b1", "5", "Employee", "Employee" },
                    { "65b30edc-edb0-4daa-bb00-0b7110fa8bb8", "2", "HR", "HR" },
                    { "ba120fdf-05db-48b7-bb25-d78003eaf1ca", "3", "Sales", "Sales" },
                    { "f227fff3-37ea-413d-af37-27e1ce3a64ba", "7", "BD", "BD" }
                });
        }
    }
}
