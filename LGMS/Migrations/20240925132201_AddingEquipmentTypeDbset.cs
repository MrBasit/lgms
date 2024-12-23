using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class AddingEquipmentTypeDbset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_EquipmentType_TypeId",
                table: "Equipments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EquipmentType",
                table: "EquipmentType");

            migrationBuilder.RenameTable(
                name: "EquipmentType",
                newName: "EquipmentTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EquipmentTypes",
                table: "EquipmentTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_EquipmentTypes_TypeId",
                table: "Equipments",
                column: "TypeId",
                principalTable: "EquipmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_EquipmentTypes_TypeId",
                table: "Equipments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EquipmentTypes",
                table: "EquipmentTypes");

            migrationBuilder.RenameTable(
                name: "EquipmentTypes",
                newName: "EquipmentType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EquipmentType",
                table: "EquipmentType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_EquipmentType_TypeId",
                table: "Equipments",
                column: "TypeId",
                principalTable: "EquipmentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
