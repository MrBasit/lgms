using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class linkAttandaceIdsWithEmployeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttandanceIdId",
                table: "Employees",
                type: "int",
                nullable: true,
                defaultValue: null);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_AttandanceIdId",
                table: "Employees",
                column: "AttandanceIdId",
                unique: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AttandanceIds_AttandanceIdId",
                table: "Employees",
                column: "AttandanceIdId",
                principalTable: "AttandanceIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_AttandanceIds_AttandanceIdId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_AttandanceIdId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AttandanceIdId",
                table: "Employees");
        }
    }
}
