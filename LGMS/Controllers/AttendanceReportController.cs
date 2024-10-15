using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using LGMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceReportController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<AttendanceReportDTO> _pagedData;
        private readonly AttendanceReportService _excelService;


        public AttendanceReportController(LgmsDbContext dbContext, AttendanceReportService excelService)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<AttendanceReportDTO>();
            _excelService = excelService;
        }

        [HttpPost("GetAttendanceReports")]
        public IActionResult GetAttendanceReports(AttendanceReportSearchModel searchModel)
        {
            if (searchModel == null)
                return BadRequest(new { message = "Invalid search criteria" });

            try
            {
                var query = _dbContext.AttendanceRecords
                    .Include(e => e.AttendanceId)
                    .Include(e => e.Status)
                    .AsQueryable();

                if (searchModel.Year > 0)
                {
                    query = query.Where(ar => ar.Date.Year == searchModel.Year);
                }
                else
                {
                    return BadRequest(new { message = "Year is required." });
                }

                if (searchModel.Month > 0)
                {
                    query = query.Where(ar => ar.Date.Month == searchModel.Month);
                }
                else
                {
                    return BadRequest(new { message = "Month is required." });
                }

                if (searchModel.MachineNames?.Any() == true)
                {
                    var lowerCaseMachineNames = searchModel.MachineNames.Select(name => name.ToLower()).ToList();
                    query = query.Where(ar => lowerCaseMachineNames.Contains(ar.AttendanceId.MachineName.ToLower()));
                }


                var attendanceRecords = query.ToList();

                var attendanceReports = new List<AttendanceReportDTO>();
                var reportService = _excelService;

                foreach (var name in searchModel.MachineNames)
                {
                    var employeeRecords = attendanceRecords
                        .Where(x => x.AttendanceId.MachineName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (employeeRecords.Any())
                    {
                        var report = reportService.GenerateReport(name, employeeRecords);
                        attendanceReports.Add(report);
                    }
                }

                if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
                {
                    attendanceReports = attendanceReports
                        .Where(e => e.Name.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                    searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
                {
                    switch (searchModel.SortDetails.SortColumn)
                    {
                        case "name":
                            attendanceReports = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                attendanceReports.OrderBy(e => e.Name).ToList() :
                                attendanceReports.OrderByDescending(e => e.Name).ToList();
                            break;
                        case "onTimes":
                            attendanceReports = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                attendanceReports.OrderBy(e => e.OnTimes).ToList() :
                                attendanceReports.OrderByDescending(e => e.OnTimes).ToList();
                            break;
                        case "lateIns":
                            attendanceReports = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                attendanceReports.OrderBy(e => e.LateIns).ToList() :
                                attendanceReports.OrderByDescending(e => e.LateIns).ToList();
                            break;
                        case "dayOffs":
                            attendanceReports = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                attendanceReports.OrderBy(e => e.DayOffs).ToList() :
                                attendanceReports.OrderByDescending(e => e.DayOffs).ToList();
                            break;
                        case "overHours":
                            attendanceReports = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                attendanceReports.OrderBy(e => e.OverHours).ToList() :
                                attendanceReports.OrderByDescending(e => e.OverHours).ToList();
                            break;
                        case "underHours":
                            attendanceReports = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                attendanceReports.OrderBy(e => e.UnderHours).ToList() :
                                attendanceReports.OrderByDescending(e => e.UnderHours).ToList();
                            break;
                        default:
                            attendanceReports = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                attendanceReports.OrderBy(e => e.Name).ToList() :
                                attendanceReports.OrderByDescending(e => e.Name).ToList();
                            break;
                    }
                }
                else
                {
                    attendanceReports = attendanceReports.OrderBy(e => e.Name).ToList();
                }

                var pagedAttendanceReportResult = _pagedData.GetPagedData(
                    attendanceReports,
                    (PagedDataRequestModel)searchModel.PaginationDetails
                );

                return Ok(pagedAttendanceReportResult);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



    }
}
