using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class AddAttandanceIdsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttandanceIds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttandanceIds");
        }
    }
}
