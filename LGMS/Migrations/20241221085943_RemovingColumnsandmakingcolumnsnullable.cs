using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class RemovingColumnsandmakingcolumnsnullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalarySlips_Employees_EmployeeId",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "Designation",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SalarySlips");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "SalarySlips",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SalarySlips_Employees_EmployeeId",
                table: "SalarySlips",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalarySlips_Employees_EmployeeId",
                table: "SalarySlips");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "SalarySlips",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "SalarySlips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "SalarySlips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SalarySlips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_SalarySlips_Employees_EmployeeId",
                table: "SalarySlips",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
