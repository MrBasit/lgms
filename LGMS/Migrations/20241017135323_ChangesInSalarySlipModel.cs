using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInSalarySlipModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "SalarySlips");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "SalarySlips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GenratedDate",
                table: "SalarySlips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IncomeTax",
                table: "SalarySlips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Loan",
                table: "SalarySlips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PayPeriod",
                table: "SalarySlips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecurityDeposit",
                table: "SalarySlips",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "GenratedDate",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "IncomeTax",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "Loan",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "PayPeriod",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "SecurityDeposit",
                table: "SalarySlips");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "SalarySlips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "SalarySlips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
