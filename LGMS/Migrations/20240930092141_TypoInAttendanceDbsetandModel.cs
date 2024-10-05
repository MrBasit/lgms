using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class TypoInAttendanceDbsetandModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_AttandanceIds_AttandanceIdId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "AttandanceIds");

            migrationBuilder.AlterColumn<int>(
                name: "AttandanceIdId",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "AttendanceIds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceIds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceIds_MachineId",
                table: "AttendanceIds",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceIds_MachineName",
                table: "AttendanceIds",
                column: "MachineName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AttendanceIds_AttandanceIdId",
                table: "Employees",
                column: "AttandanceIdId",
                principalTable: "AttendanceIds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_AttendanceIds_AttandanceIdId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "AttendanceIds");

            migrationBuilder.AlterColumn<int>(
                name: "AttandanceIdId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AttandanceIds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    MachineName = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttandanceIds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttandanceIds_MachineId",
                table: "AttandanceIds",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttandanceIds_MachineName",
                table: "AttandanceIds",
                column: "MachineName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AttandanceIds_AttandanceIdId",
                table: "Employees",
                column: "AttandanceIdId",
                principalTable: "AttandanceIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
