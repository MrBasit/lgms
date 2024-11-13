using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class CHangingInTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractPackagesInformation_Contracts_ContractId",
                table: "ContractPackagesInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientInformationId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_ContractPackagesInformation_ContractId",
                table: "ContractPackagesInformation");

            migrationBuilder.RenameColumn(
                name: "ClientInformationId",
                table: "Contracts",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_ClientInformationId",
                table: "Contracts",
                newName: "IX_Contracts_ClientId");

            migrationBuilder.AlterColumn<int>(
                name: "ContractId",
                table: "ContractPackagesInformation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPackagesInformation_ContractId",
                table: "ContractPackagesInformation",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractPackagesInformation_Contracts_ContractId",
                table: "ContractPackagesInformation",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractPackagesInformation_Contracts_ContractId",
                table: "ContractPackagesInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_ContractPackagesInformation_ContractId",
                table: "ContractPackagesInformation");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Contracts",
                newName: "ClientInformationId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts",
                newName: "IX_Contracts_ClientInformationId");

            migrationBuilder.AlterColumn<int>(
                name: "ContractId",
                table: "ContractPackagesInformation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractPackagesInformation_ContractId",
                table: "ContractPackagesInformation",
                column: "ContractId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractPackagesInformation_Contracts_ContractId",
                table: "ContractPackagesInformation",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientInformationId",
                table: "Contracts",
                column: "ClientInformationId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
