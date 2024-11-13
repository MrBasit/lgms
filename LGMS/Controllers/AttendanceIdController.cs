using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceIdController : ControllerBase
    {
        private LgmsDbContext _dbContext;
        private PagedData<AttendanceId> _pagedData;

        public AttendanceIdController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<AttendanceId>();
        }
        [HttpPost("GetAttendanceIdsWithFilters")]
        public IActionResult GetAttendanceIdsWithFilters(BaseSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new{message = "Invalid search criteria" });
            var attendanceIds = new List<AttendanceId>();
            try
            {
                attendanceIds = _dbContext.AttendanceIds.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (!attendanceIds.Any()) return NotFound(new{message="No attendance ids are there"});
            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
               searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "employeeNumber":
                        attendanceIds = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            attendanceIds.OrderBy(a => a.MachineId).ToList() :
                            attendanceIds.OrderByDescending(a => a.MachineId).ToList();
                        break;
                    case "employeeName":
                        attendanceIds = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            attendanceIds.OrderBy(a => a.MachineName).ToList() :
                            attendanceIds.OrderByDescending(a => a.MachineName).ToList();
                        break;
                    default:
                        attendanceIds = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            attendanceIds.OrderBy(a => a.MachineId).ToList() :
                            attendanceIds.OrderByDescending(a => a.MachineId).ToList();
                        break;
                }
            }
            else
            {
                attendanceIds = attendanceIds.OrderBy(a => a.MachineId).ToList();
            }
            var pagedAttendanceIdsResult = _pagedData.GetPagedData(
                attendanceIds,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedAttendanceIdsResult);
        }

        [HttpGet("GetAttendanceIdById")]
        public IActionResult GetAttendanceIdById(int id)
        {
            var attendanceId = _dbContext.AttendanceIds
                .SingleOrDefault(a => a.Id == id);
            if (attendanceId == null) return BadRequest(new { message = string.Format("AttendanceId with id {0} doesn't exist", attendanceId) });
            return Ok(attendanceId);
        }

        [HttpGet("GetAttendanceIds")]
        public IActionResult GetAttendanceIds()
        {
            var attendanceIds = _dbContext.AttendanceIds.ToList();
            return Ok(attendanceIds);
        }

        [HttpPost("AddAttendanceId")]
        public IActionResult AddAttendanceId(AttendanceIdAddModel attendanceIdDetails)
        {
            if (_dbContext.AttendanceIds.FirstOrDefault(a => a.MachineName.ToUpper() == attendanceIdDetails.MachineName.ToUpper()) != null)
            {
                return BadRequest(new { message = "AttendanceId with this Machine Name already Exist" });
            }
            if (_dbContext.AttendanceIds.FirstOrDefault(a => a.MachineId == attendanceIdDetails.MachineId) != null)
            {
                return BadRequest(new { message = "AttendanceId with this Machine Id already Exist" });
            }
            try
            {
                AttendanceId attendanceId = new AttendanceId()
                {
                    MachineId = attendanceIdDetails.MachineId,
                    MachineName = attendanceIdDetails.MachineName,
                };
                _dbContext.AttendanceIds.Add(attendanceId);
                _dbContext.SaveChanges();
                return Ok(attendanceId);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    innerMessage = ex.InnerException != null ? ex.InnerException.Message : ""
                });
            }
        }

        [HttpPost("EditAttendanceId")]
        public IActionResult EditAttendanceId(AttendanceIdEditModel attendanceIdDetails)
        {
            var existingAttendanceId = _dbContext.AttendanceIds.FirstOrDefault(a => a.Id == attendanceIdDetails.Id);

            if (existingAttendanceId == null)
            {
                return NotFound("AttendanceId not Found");
            }

            if (_dbContext.AttendanceIds.Any(a => a.MachineName.ToUpper() == attendanceIdDetails.MachineName.ToUpper() && a.Id != attendanceIdDetails.Id))
            {
                return BadRequest(new
                {
                    message = "AttendanceId with this Machine Name already Exist"
                });
            }
            if (_dbContext.AttendanceIds.Any(a => a.MachineId == attendanceIdDetails.MachineId && a.Id != attendanceIdDetails.Id))
            {
                return BadRequest(new
                {
                    message = "AttendanceId with this Machine Id already Exist"
                });
            }
            try
            {
                existingAttendanceId.MachineId = attendanceIdDetails.MachineId;
                existingAttendanceId.MachineName = attendanceIdDetails.MachineName;
                _dbContext.SaveChanges();
                return Ok(existingAttendanceId);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    innerMessage = ex.InnerException != null ? ex.InnerException.Message : ""
                });
            }
        }

    }
}
