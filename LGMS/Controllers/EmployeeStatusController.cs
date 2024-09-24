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
        public IActionResult GetEmployeeStatusesWithFilters(EmployeeStatusesSearchModel employeeStatusesSearchModel)
        {
            if (employeeStatusesSearchModel == null) return BadRequest("Invalid search criteria");

            var employeeStatuses = new List<EmployeeStatus>();

            try
            {
                employeeStatuses = _dbContext.EmployeeStatus
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!employeeStatuses.Any()) return NotFound("Employee status Not Found");

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
    }
}
