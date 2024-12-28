using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeStatusController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<EmployeeStatus> _pagedData;

        public EmployeeStatusController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<EmployeeStatus>();

        }

        [HttpGet("GetEmployeeStatuses")]
        public IActionResult GetEmployeeStatuses()
        {
            var employeeStatuses = _dbContext.EmployeeStatus.ToList();
            return Ok(employeeStatuses);
        }

        [HttpPost("GetEmployeeStatusesWithFilters")]
        public IActionResult GetEmployeeStatusesWithFilters(BaseSearchModel employeeStatusesSearchModel)
        {
            if (employeeStatusesSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var employeeStatuses = new List<EmployeeStatus>();

            try
            {
                employeeStatuses = _dbContext.EmployeeStatus
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
            if (!employeeStatuses.Any()) return NotFound(new { message = "Employee status Not Found" });

            if (!string.IsNullOrEmpty(employeeStatusesSearchModel.SearchDetails.SearchTerm))
            {
                employeeStatuses = employeeStatuses.Where(e =>
                    e.Title.ToUpper().Contains(employeeStatusesSearchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }
            if (!string.IsNullOrEmpty(employeeStatusesSearchModel.SortDetails.SortColumn) &&
                employeeStatusesSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (employeeStatusesSearchModel.SortDetails.SortColumn)
                {
                    case "id":
                        employeeStatuses = employeeStatusesSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            employeeStatuses.OrderBy(e => e.Id).ToList() :
                            employeeStatuses.OrderByDescending(e => e.Id).ToList();
                        break;
                    case "title":
                        employeeStatuses = employeeStatusesSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            employeeStatuses.OrderBy(e => e.Title).ToList() :
                            employeeStatuses.OrderByDescending(e => e.Title).ToList();
                        break;
                    default:
                        employeeStatuses = employeeStatusesSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            employeeStatuses.OrderBy(e => e.Title).ToList() :
                            employeeStatuses.OrderByDescending(e => e.Title).ToList();
                        break;
                }
            }
            else
            {
                employeeStatuses = employeeStatuses.OrderBy(e => e.Id).ToList();
            }

            var pagedEmployeeStatusesResult = _pagedData.GetPagedData(
                employeeStatuses,
                (PagedDataRequestModel)employeeStatusesSearchModel.PaginationDetails
            );

            return Ok(pagedEmployeeStatusesResult);
        }
        [HttpGet("GetEmployeeStatusById")]
        public IActionResult GetEmployeeStatusById(int id)
        {
            var employeeStatus = _dbContext.EmployeeStatus
                .SingleOrDefault(d => d.Id == id);
            if (employeeStatus == null) return BadRequest(new { message = string.Format("EmployeeStatus with id {0} doesn't exist", id) });
            return Ok(employeeStatus);
        }

        [HttpPost("EditEmployeeStatus")]
        public IActionResult EditEmployeeStatus(EmployeeStatusEditModel statusDetails)
        {
            var existingStatus = _dbContext.EmployeeStatus.FirstOrDefault(d => d.Id == statusDetails.Id);

            if (existingStatus == null)
            {
                return NotFound(new { message = "EmployeeStatus not Found" });
            }

            if (_dbContext.EmployeeStatus.Any(d => d.Title.ToUpper() == statusDetails.Title.ToUpper() && d.Id != statusDetails.Id))
            {
                return BadRequest(new
                {
                    message = "EmployeeStatus with this Title already Exist"
                });
            }
            try
            {
                existingStatus.Title = statusDetails.Title;
                _dbContext.SaveChanges();
                return Ok(existingStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }
        [HttpPost("DeleteEmployeeStatus")]
        public IActionResult DeleteEmployeeStatus([FromBody] int id)
        {
            var existingStatus = _dbContext.EmployeeStatus.FirstOrDefault(d => d.Id == id);

            if (existingStatus == null)
            {
                return NotFound(new { message = "EmployeeStatus not Found" });
            }
            if(_dbContext.Employees.Any(e => e.Status.Id == existingStatus.Id))
            {
                return BadRequest(new { message = $"{existingStatus.Title} is in use and it can't be delete." });
            }

            try
            {
                _dbContext.EmployeeStatus.Remove(existingStatus);
                _dbContext.SaveChanges();
                return Ok(new { message = $"{existingStatus.Title} has been deleted." });
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
