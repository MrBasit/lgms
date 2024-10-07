using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<Department> _pagedData;

        public DepartmentController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Department>();

        }

        [HttpGet("GetDepartments")]
        public IActionResult GetDepartments()
        {
            var departments = _dbContext.Departments.ToList();
            return Ok(departments);
        }

        [HttpPost("GetDepartmentsWithFilters")]
        public IActionResult GetDepartmentsWithFilters(DepartmentsSearchModel departmentSearchModel)
        {
            if (departmentSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var departments = new List<Department>();

            try
            {
                departments = _dbContext.Departments
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!departments.Any()) return NotFound(new { message = "departments Not Found" });

            if (!string.IsNullOrEmpty(departmentSearchModel.SearchDetails.SearchTerm))
            {
                departments = departments.Where(e =>
                    e.Name.ToUpper().Contains(departmentSearchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }
            if (!string.IsNullOrEmpty(departmentSearchModel.SortDetails.SortColumn) &&
                departmentSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (departmentSearchModel.SortDetails.SortColumn)
                {
                    case "id":
                        departments = departmentSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    departments.OrderBy(e => e.Id).ToList() :
                                    departments.OrderByDescending(e => e.Id).ToList();
                        break;
                    case "name":
                        departments = departmentSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    departments.OrderBy(e => e.Name).ToList() :
                                    departments.OrderByDescending(e => e.Name).ToList();
                        break;
                    default:
                        departments = departmentSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    departments.OrderBy(e => e.Name).ToList() :
                                    departments.OrderByDescending(e => e.Name).ToList();
                        break;
                }
            }
            else
            {
                departments = departments.OrderBy(e => e.Id).ToList();
            }

            var pagedDepartmentsResult = _pagedData.GetPagedData(
            departments,
                (PagedDataRequestModel)departmentSearchModel.PaginationDetails
            );

            return Ok(pagedDepartmentsResult);
        }
    }
}
