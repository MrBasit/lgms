using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using LGMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceRecordController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<AttendanceRecord> _pagedData;

        public AttendanceRecordController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<AttendanceRecord>();
        }

        [HttpPost("GetAttendanceRecords")]
        public IActionResult GetAttendanceRecords(AttendanceRecordSearchModel searchModel)
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
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
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
                    case "name":
                        attendanceRecords = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            attendanceRecords.OrderBy(e => e.AttendanceId.MachineName).ToList() :
                            attendanceRecords.OrderByDescending(e => e.AttendanceId.MachineName).ToList();
                        break;
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
                        attendanceRecords = attendanceRecords
                            .OrderBy(e => e.AttendanceId.MachineName) 
                            .ThenBy(e => e.Date)                  
                            .ToList();
                        break;
                }
            }
            else
            {
                attendanceRecords = attendanceRecords.OrderBy(e => e.AttendanceId.MachineName).ThenBy(e => e.Date).ToList();
            }

            var pagedAttendanceRecordResult = _pagedData.GetPagedData(
                attendanceRecords,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedAttendanceRecordResult);

        }
        [HttpGet("GetAttendanceYearsRange")]
        public ActionResult GetAttendanceYearsRange()
        {
            var firstAscendingYear = _dbContext.AttendanceRecords
                .OrderBy(ar => ar.Date)
                .Select(ar => ar.Date.Year)
                .FirstOrDefault();

            var firstDescendingYear = _dbContext.AttendanceRecords
                .OrderByDescending(ar => ar.Date)
                .Select(ar => ar.Date.Year)
                .FirstOrDefault();

            if (firstAscendingYear == 0 || firstDescendingYear == 0)
            {
                return Ok(new int[] { });
            }

            var yearsRange = Enumerable.Range(firstAscendingYear, firstDescendingYear - firstAscendingYear + 1).ToArray();

            return Ok(yearsRange);
        }
        [HttpGet("GetUnderHoursWithEmployees")]
        public ActionResult GetUnderHoursWithEmployees()
        {
            var currentDate = DateTime.Now;
            var previousMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1);
            var previousMonthEnd = new DateTime(currentDate.Year, currentDate.Month, 1).AddDays(-1);

            var employeesWithUnderHours = _dbContext.AttendanceRecords
                .Where(a => a.Date >= previousMonthStart && a.Date <= previousMonthEnd)
                .GroupBy(a => a.AttendanceId)
                .Select(group => new
                {
                    AttendanceId = group.Key,
                    TotalUnderHours = group.Sum(a => a.UnderHours),
                })
                .OrderByDescending(e => e.TotalUnderHours)
                .ToList();

            var result = employeesWithUnderHours.Select(e =>
            {
                var employee = _dbContext.Employees
                                 .FirstOrDefault(emp => emp.AttendanceId == e.AttendanceId);
                var hourLabel = e.TotalUnderHours == 1 ? "hour" : "hours";
                return $"{employee.Name} - {e.TotalUnderHours} {hourLabel}";
            }).ToList();

            return Ok(result);
        }
        [HttpGet("GetOverHoursWithEmployees")]
        public ActionResult GetOverHoursWithEmployees()
        {
            var currentDate = DateTime.Now;
            var previousMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1);
            var previousMonthEnd = new DateTime(currentDate.Year, currentDate.Month, 1).AddDays(-1);

            var employeesWithOverHours = _dbContext.AttendanceRecords
                .Where(a => a.Date >= previousMonthStart && a.Date <= previousMonthEnd)
                .GroupBy(a => a.AttendanceId)
                .Select(group => new
                {
                    AttendanceId = group.Key,
                    TotalOverHours = group.Sum(a => a.OverHours),
                })
                .OrderByDescending(e => e.TotalOverHours)
                .ToList();

            var result = employeesWithOverHours.Select(e => {
                var employee = _dbContext.Employees
                                 .FirstOrDefault(emp => emp.AttendanceId == e.AttendanceId);
                var hourLabel = e.TotalOverHours == 1 ? "hour" : "hours";
                return $"{employee.Name} - {e.TotalOverHours} {hourLabel}";
            }).ToList();

            return Ok(result);
        }
        [HttpGet("GetDaysOffWithEmployees")]
        public ActionResult GetDaysOffWithEmployees()
        {
            try
            {
                var currentDate = DateTime.Now;
                var startOfYear = new DateTime(currentDate.Year, 1, 1);

                var employeesWithAttendance = _dbContext.Employees
                    .Where(emp => emp.Status.Title == "Active" && emp.AttendanceId != null)
                    .GroupJoin(
                        _dbContext.AttendanceRecords
                            .Where(a => a.Date >= startOfYear && a.Date <= currentDate),
                        emp => emp.AttendanceId,
                        att => att.AttendanceId,
                        (employee, attendanceGroup) => new
                        {
                            Employee = employee,
                            TotalDaysOff = attendanceGroup.Count(a => a.Status != null && a.Status.Title == "Day Off")
                        }
                    )
                    .OrderByDescending(e => e.TotalDaysOff)
                    .ToList();

                var result = employeesWithAttendance.Select(e =>
                {
                    var dayLabel = e.TotalDaysOff == 1 ? "day" : "days";
                    return $"{e.Employee.Name} - {e.TotalDaysOff} {dayLabel}";
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }




    }
}
