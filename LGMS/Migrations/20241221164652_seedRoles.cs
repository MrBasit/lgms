using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class seedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0576556f-5296-4c55-ab0c-8a7fec10c15f", "3", "Sales", "Sales" },
                    { "276d6687-17d1-4b4a-a39e-8f10849aa91a", "1", "Admin", "Admin" },
                    { "4b646391-eea4-4b7a-80e0-586ab175d406", "4", "Stores", "Stores" },
                    { "66d74431-7382-4dac-829d-e297e676b4eb", "2", "HR", "HR" },
                    { "a499cc23-9194-460d-80e1-9db90c6788c6", "5", "Employee", "Employee" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0576556f-5296-4c55-ab0c-8a7fec10c15f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "276d6687-17d1-4b4a-a39e-8f10849aa91a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4b646391-eea4-4b7a-80e0-586ab175d406");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "66d74431-7382-4dac-829d-e297e676b4eb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a499cc23-9194-460d-80e1-9db90c6788c6");
        }
    }
}
