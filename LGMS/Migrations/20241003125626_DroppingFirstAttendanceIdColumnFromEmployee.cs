using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class DroppingFirstAttendanceIdColumnFromEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_AttendanceIds_AttandanceIdId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AttendanceId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "AttandanceIdId",
                table: "Employees",
                newName: "AttendanceIdId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_AttandanceIdId",
                table: "Employees",
                newName: "IX_Employees_AttendanceIdId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AttendanceIds_AttendanceIdId",
                table: "Employees",
                column: "AttendanceIdId",
                principalTable: "AttendanceIds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_AttendanceIds_AttendanceIdId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "AttendanceIdId",
                table: "Employees",
                newName: "AttandanceIdId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_AttendanceIdId",
                table: "Employees",
                newName: "IX_Employees_AttandanceIdId");

            migrationBuilder.AddColumn<int>(
                name: "AttendanceId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AttendanceIds_AttandanceIdId",
                table: "Employees",
                column: "AttandanceIdId",
                principalTable: "AttendanceIds",
                principalColumn: "Id");
        }
    }
}
