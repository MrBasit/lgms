using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class AddingEmployeeInSalarySlip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "SalarySlips",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalarySlips_EmployeeId",
                table: "SalarySlips",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalarySlips_Employees_EmployeeId",
                table: "SalarySlips",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalarySlips_Employees_EmployeeId",
                table: "SalarySlips");

            migrationBuilder.DropIndex(
                name: "IX_SalarySlips_EmployeeId",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "SalarySlips");
        }
    }
}
