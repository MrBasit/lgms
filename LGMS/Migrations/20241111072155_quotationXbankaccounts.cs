using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class quotationXbankaccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Quotations_QuotationId",
                table: "BankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Clients_ClientInformationId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_QuotationId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "QuotationId",
                table: "BankAccounts");

            migrationBuilder.RenameColumn(
                name: "ClientInformationId",
                table: "Quotations",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Quotations_ClientInformationId",
                table: "Quotations",
                newName: "IX_Quotations_ClientId");

            migrationBuilder.CreateTable(
                name: "BankAccountQuotation",
                columns: table => new
                {
                    BankAccountsId = table.Column<int>(type: "int", nullable: false),
                    QuotationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccountQuotation", x => new { x.BankAccountsId, x.QuotationsId });
                    table.ForeignKey(
                        name: "FK_BankAccountQuotation_BankAccounts_BankAccountsId",
                        column: x => x.BankAccountsId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BankAccountQuotation_Quotations_QuotationsId",
                        column: x => x.QuotationsId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountQuotation_QuotationsId",
                table: "BankAccountQuotation",
                column: "QuotationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Clients_ClientId",
                table: "Quotations",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Clients_ClientId",
                table: "Quotations");

            migrationBuilder.DropTable(
                name: "BankAccountQuotation");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Quotations",
                newName: "ClientInformationId");

            migrationBuilder.RenameIndex(
                name: "IX_Quotations_ClientId",
                table: "Quotations",
                newName: "IX_Quotations_ClientInformationId");

            migrationBuilder.AddColumn<int>(
                name: "QuotationId",
                table: "BankAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_QuotationId",
                table: "BankAccounts",
                column: "QuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Quotations_QuotationId",
                table: "BankAccounts",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Clients_ClientInformationId",
                table: "Quotations",
                column: "ClientInformationId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
