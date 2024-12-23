using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class ChangingOnDeleteActionInForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.DropForeignKey(
               name: "FK_Employees_Departments_DepartmentId",
               table: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Designations_DesignationId",
                table: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Designations_DesignationId",
                table: "Employees",
                column: "DesignationId",
                principalTable: "Designations",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeStatus_StatusId",
                table: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeStatus_StatusId",
                table: "Employees",
                column: "StatusId",
                principalTable: "EmployeeStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
               name: "FK_Employees_AttendanceIds_AttendanceIdId",
               table: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AttendanceIds_AttendanceIdId",
                table: "Employees",
                column: "AttendanceIdId",
                principalTable: "AttendanceIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEquipment_Employees_AssigneesId",
                table: "EmployeeEquipment");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEquipment_Equipments_EquipmentsId",
                table: "EmployeeEquipment");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEquipment_Employees_AssigneesId",
                table: "EmployeeEquipment",
                column: "AssigneesId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEquipment_Equipments_EquipmentsId",
                table: "EmployeeEquipment",
                column: "EquipmentsId",
                principalTable: "Equipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Equipments_ParentEquipmentId",
                table: "Equipments");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Equipments_ParentEquipmentId",
                table: "Equipments",
                column: "ParentEquipmentId",
                principalTable: "Equipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_EquipmentStatus_StatusId",
                table: "Equipments");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_EquipmentStatus_StatusId",
                table: "Equipments",
                column: "StatusId",
                principalTable: "EquipmentStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_EquipmentTypes_TypeId",
                table: "Equipments");

            migrationBuilder.AddForeignKey(
                 name: "FK_Equipments_EquipmentTypes_TypeId",
                table: "Equipments",
                column: "TypeId",
                principalTable: "EquipmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Manufacturers_ManufacturerId",
                table: "Equipments");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Manufacturers_ManufacturerId",
                table: "Equipments",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Vendors_VendorId",
                table: "Equipments");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Vendors_VendorId",
                table: "Equipments",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
               name: "FK_AttendanceRecords_AttendanceIds_AttendanceIdId",
               table: "AttendanceRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_AttendanceIds_AttendanceIdId",
                table: "AttendanceRecords",
                column: "AttendanceIdId",
                principalTable: "AttendanceIds",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropForeignKey(
               name: "FK_AttendanceRecords_AttendanceRecordStatuses_StatusId",
               table: "AttendanceRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_AttendanceRecordStatuses_StatusId",
                table: "AttendanceRecords",
                column: "StatusId",
                principalTable: "AttendanceRecordStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
