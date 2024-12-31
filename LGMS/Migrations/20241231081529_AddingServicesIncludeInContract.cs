using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class AddingServicesIncludeInContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServicesInclude",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServicesInclude",
                table: "Contracts");
        }
    }
}
