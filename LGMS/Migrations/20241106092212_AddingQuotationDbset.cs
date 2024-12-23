using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class AddingQuotationDbset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuotationId",
                table: "BankAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientInformationId = table.Column<int>(type: "int", nullable: false),
                    PackageInformationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotations_Clients_ClientInformationId",
                        column: x => x.ClientInformationId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Quotations_QuotationPackagesInformation_PackageInformationId",
                        column: x => x.PackageInformationId,
                        principalTable: "QuotationPackagesInformation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_QuotationId",
                table: "BankAccounts",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_ClientInformationId",
                table: "Quotations",
                column: "ClientInformationId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_PackageInformationId",
                table: "Quotations",
                column: "PackageInformationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Quotations_QuotationId",
                table: "BankAccounts",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Quotations_QuotationId",
                table: "BankAccounts");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_QuotationId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "QuotationId",
                table: "BankAccounts");
        }
    }
}
