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
                return BadRequest(ex.Message);
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



    }
}
