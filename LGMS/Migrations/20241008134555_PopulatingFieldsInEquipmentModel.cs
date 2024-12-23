using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    public partial class PopulatingFieldsInEquipmentModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentEquipmentId",
                table: "Equipments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_ParentEquipmentId",
                table: "Equipments",
                column: "ParentEquipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Equipments_ParentEquipmentId",
                table: "Equipments",
                column: "ParentEquipmentId",
                principalTable: "Equipments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Equipments_ParentEquipmentId",
                table: "Equipments");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_ParentEquipmentId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "ParentEquipmentId",
                table: "Equipments");
        }
    }
}
