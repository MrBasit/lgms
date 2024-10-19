using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly ExcelService _DownloadService;
        private LgmsDbContext _dbContext;

        public ExportController(ExcelService DownloadService, LgmsDbContext dbContext)
        {
            _DownloadService = DownloadService;
            _dbContext = dbContext;
        }

        [HttpPost("DownloadEquipment")]
        public IActionResult DownloadEquipment(EquipmentExportSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });
            var equipments = new List<Equipment>();

            try
            {
                equipments = _dbContext.Equipments
                    .Include(e => e.Status)
                    .Include(e => e.Manufacturer)
                    .Include(e => e.Vendor)
                    .Include(e => e.Type)
                    .Include(e => e.Assignees)
                    .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            if (!equipments.Any()) return NotFound(new { message = "Equipments Not Found" });

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                var searchTerm = searchModel.SearchDetails.SearchTerm.ToUpper();
                equipments = equipments.Where(e =>
                    e.Type.Title.ToUpper().Contains(searchTerm) ||
                    e.Manufacturer.Name.ToUpper().Contains(searchTerm) ||
                    e.Vendor.Name.ToUpper().Contains(searchTerm) ||
                    e.Assignees.Any(a => a.Name.ToUpper().Contains(searchTerm))
                ).ToList();
            }
            var equipmentWithSelectedStatuses = new List<Equipment>();
            foreach (var status in searchModel.Statuses)
            {
                equipmentWithSelectedStatuses.AddRange(equipments.Where(x => x.Status.Id == status.Id).ToList());
            }

            equipments = equipmentWithSelectedStatuses;

            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "number":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Number).ToList()
                            : equipments.OrderByDescending(e => e.Number).ToList();
                        break;
                    case "manufacturer":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Manufacturer).ToList()
                            : equipments.OrderByDescending(e => e.Manufacturer).ToList();
                        break;
                    case "assignees":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Assignees).ToList()
                            : equipments.OrderByDescending(e => e.Assignees).ToList();
                        break;
                    case "status":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Status).ToList()
                            : equipments.OrderByDescending(e => e.Status).ToList();
                        break;
                    case "vendor":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Vendor).ToList()
                            : equipments.OrderByDescending(e => e.Vendor).ToList();
                        break;
                    case "type":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Type).ToList()
                            : equipments.OrderByDescending(e => e.Type).ToList();
                        break;
                    default:
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Number).ToList()
                            : equipments.OrderByDescending(e => e.Number).ToList();
                        break;
                }
            }
            else
            {
                equipments = equipments.OrderBy(e => e.Number).ToList();
            }

            try
            {
                
                var fileContents = _DownloadService.GenerateEquipmentExcelFile(equipments);
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "equipment_data.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPost("DownloadAttendanceRecords")]
        public IActionResult DownloadAttendanceRecords(AttendanceRecordExportSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var attendanceRecords = new List<AttendanceRecord>();

            try
            {
                attendanceRecords = _dbContext.AttendanceRecords
                    .Include(e => e.AttendanceId)
                    .Include(e => e.Status)
                    .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            if (searchModel.Year > 0)
            {
                attendanceRecords = attendanceRecords.Where(ar => ar.Date.Year == searchModel.Year).ToList();
            }
            else
            {
                return BadRequest(new { message = "Year is required" });
            }

            if (searchModel.Month > 0)
            {
                attendanceRecords = attendanceRecords.Where(ar => ar.Date.Month == searchModel.Month).ToList();
            }
            else
            {
                return BadRequest(new { message = "Month is required." });
            }
            var recordsWithIncludedNames = new List<AttendanceRecord>();

            foreach (var name in searchModel.MachineNames)
            {
                recordsWithIncludedNames.AddRange(attendanceRecords.Where(x => x.AttendanceId.MachineName.ToUpper() == name.ToUpper()).ToList());
            }

            attendanceRecords = recordsWithIncludedNames;

            if (searchModel.Date > 0)
            {
                attendanceRecords = attendanceRecords.Where(ar => ar.Date.Day == searchModel.Date).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                attendanceRecords = attendanceRecords.Where(e =>
                    e.AttendanceId.MachineName.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Status.Title.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }
            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "date":
                        attendanceRecords = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            attendanceRecords.OrderBy(e => e.Date).ToList() :
                            attendanceRecords.OrderByDescending(e => e.Date).ToList();
                        break;
                    case "status":
                        attendanceRecords = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            attendanceRecords.OrderBy(e => e.Status.Title).ToList() :
                            attendanceRecords.OrderByDescending(e => e.Status.Title).ToList();
                        break;
                    case "overHours":
                        attendanceRecords = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            attendanceRecords.OrderBy(e => e.OverHours).ToList() :
                            attendanceRecords.OrderByDescending(e => e.OverHours).ToList();
                        break;
                    case "underHours":
                        attendanceRecords = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            attendanceRecords.OrderBy(e => e.UnderHours).ToList() :
                            attendanceRecords.OrderByDescending(e => e.UnderHours).ToList();
                        break;
                    default:
                        attendanceRecords = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            attendanceRecords.OrderBy(e => e.AttendanceId.MachineName).ToList() :
                            attendanceRecords.OrderByDescending(e => e.AttendanceId.MachineName).ToList();
                        break;
                }
            }
            else
            {
                attendanceRecords = attendanceRecords.OrderBy(e => e.AttendanceId.MachineName).ToList();
            }

            var excelFile = _DownloadService.GenerateAttendanceRecordExcelFile(attendanceRecords);

            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceRecords.xlsx");
        }


    }
}
