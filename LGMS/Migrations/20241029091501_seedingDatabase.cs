using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class seedingDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "EmployeeStatus",
            columns: new[] { "Title" },
            values: new object[,]
            {
                { "Active" },
                { "Inactive" },
                { "Fired" },
                { "Left" },
                { "Deleted" }
            });
            migrationBuilder.InsertData(
            table: "EquipmentStatus",
            columns: new[] { "Title" },
            values: new object[,]
            {
                { "Functional" },
                { "Running" },
                { "Discard" },
                { "Repaired" },
                { "Delivered" },
                { "Deleted" }
            });
            migrationBuilder.InsertData(
            table: "AttendanceRecordStatuses",
            columns: new[] { "Title" },
            values: new object[,]
            {
                { "On Time" },
                { "Day Off" },
                { "On Leave" },
                { "Holiday" },
                { "Weekend" },
                { "Late In" },
                { "Extra Day" }
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
