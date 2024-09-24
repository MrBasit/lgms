using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class addEquipmentTypeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Equipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EquipmentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_TypeId",
                table: "Equipments",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_EquipmentType_TypeId",
                table: "Equipments",
                column: "TypeId",
                principalTable: "EquipmentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_EquipmentType_TypeId",
                table: "Equipments");

            migrationBuilder.DropTable(
                name: "EquipmentType");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_TypeId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Equipments");
        }
    }
}
