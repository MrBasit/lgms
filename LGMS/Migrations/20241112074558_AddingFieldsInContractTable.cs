using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class AddingFieldsInContractTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InitialDate",
                table: "Contracts",
                newName: "StartDate");

            migrationBuilder.AddColumn<int>(
                name: "ContractAmount",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractAmount",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Contracts",
                newName: "InitialDate");
        }
    }
}
