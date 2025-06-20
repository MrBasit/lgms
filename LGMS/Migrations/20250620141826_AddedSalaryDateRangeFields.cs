using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class AddedSalaryDateRangeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PayEndDate",
                table: "SalarySlips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PayStartDate",
                table: "SalarySlips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE SalarySlips
                SET PayStartDate = DATEFROMPARTS(YEAR(PayPeriod), MONTH(PayPeriod), 1),
                PayEndDate = EOMONTH(PayPeriod)
                Where PayStartDate is null and PayEndDate is null"
            );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayEndDate",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "PayStartDate",
                table: "SalarySlips");
        }
    }
}
