using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    public partial class UpdateDeleteBehaviorToNoAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientInformationId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ContractStatuses_StatusId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ContractTypes_TypeId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractPackagesInformation_Contracts_ContractId",
                table: "ContractPackagesInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_Expiration_Contracts_ContractId",
                table: "Expiration");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BankAccounts_BankAccountId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Contracts_ContractId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationPackagesInformation_Quotations_QuotationId",
                table: "QuotationPackagesInformation");

            // Re-add foreign keys with NoAction on delete behavior
            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientInformationId",
                table: "Contracts",
                column: "ClientInformationId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ContractStatuses_StatusId",
                table: "Contracts",
                column: "StatusId",
                principalTable: "ContractStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ContractTypes_TypeId",
                table: "Contracts",
                column: "TypeId",
                principalTable: "ContractTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractPackagesInformation_Contracts_ContractId",
                table: "ContractPackagesInformation",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Expiration_Contracts_ContractId",
                table: "Expiration",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_BankAccounts_BankAccountId",
                table: "Payments",
                column: "BankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Contracts_ContractId",
                table: "Payments",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationPackagesInformation_Quotations_QuotationId",
                table: "QuotationPackagesInformation",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the modified foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientInformationId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ContractStatuses_StatusId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ContractTypes_TypeId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractPackagesInformation_Contracts_ContractId",
                table: "ContractPackagesInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_Expiration_Contracts_ContractId",
                table: "Expiration");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BankAccounts_BankAccountId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Contracts_ContractId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationPackagesInformation_Quotations_QuotationId",
                table: "QuotationPackagesInformation");

            // Re-add the original foreign keys with Cascade delete behavior
            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientInformationId",
                table: "Contracts",
                column: "ClientInformationId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ContractStatuses_StatusId",
                table: "Contracts",
                column: "StatusId",
                principalTable: "ContractStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ContractTypes_TypeId",
                table: "Contracts",
                column: "TypeId",
                principalTable: "ContractTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractPackagesInformation_Contracts_ContractId",
                table: "ContractPackagesInformation",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Expiration_Contracts_ContractId",
                table: "Expiration",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_BankAccounts_BankAccountId",
                table: "Payments",
                column: "BankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Contracts_ContractId",
                table: "Payments",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationPackagesInformation_Quotations_QuotationId",
                table: "QuotationPackagesInformation",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
