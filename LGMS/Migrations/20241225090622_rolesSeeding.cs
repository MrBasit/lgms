using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class rolesSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "811ade0d-b1b1-4f27-b4c6-9ad7adfd2fd7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b3bd8dfa-9542-415c-9ff1-5a389ccc0bbf");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e001602a-3a66-4c95-9921-162daff54f23");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "eea61168-43ee-4ede-9484-6eeb3ac99cf4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f75b41fd-36f8-41c5-b6bf-558a888d6a11");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                    { "811ade0d-b1b1-4f27-b4c6-9ad7adfd2fd7", "2", "HR", "HR" },
                    { "b3bd8dfa-9542-415c-9ff1-5a389ccc0bbf", "5", "Employee", "Employee" },
                    { "e001602a-3a66-4c95-9921-162daff54f23", "1", "Admin", "Admin" },
                    { "eea61168-43ee-4ede-9484-6eeb3ac99cf4", "4", "Stores", "Stores" },
                    { "f75b41fd-36f8-41c5-b6bf-558a888d6a11", "3", "Sales", "Sales" }
                });
        }
    }
}
