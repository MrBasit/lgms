using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGMS.Migrations
{
    /// <inheritdoc />
    public partial class AddingDbSetAttendanceRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceRecordStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecordStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttendanceIdId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckIns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckOuts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    RequiredTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ActualTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    UnderHours = table.Column<int>(type: "int", nullable: false),
                    OverHours = table.Column<int>(type: "int", nullable: false),
                    IsRecordOk = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_AttendanceIds_AttendanceIdId",
                        column: x => x.AttendanceIdId,
                        principalTable: "AttendanceIds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_AttendanceRecordStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "AttendanceRecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_AttendanceIdId",
                table: "AttendanceRecords",
                column: "AttendanceIdId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_StatusId",
                table: "AttendanceRecords",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "AttendanceRecordStatuses");
        }
    }
}
