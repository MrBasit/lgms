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
            if (!departments.Any()) return NotFound(new { message = "No departments are there" });

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
        [HttpGet("GetDepartmentById")]
        public IActionResult GetDepartmentById(int id)
        {
            var department = _dbContext.Departments
                .SingleOrDefault(d => d.Id == id);
            if (department == null) return BadRequest(new { message = string.Format("Department with id {0} doesn't exist", id) });
            return Ok(department);
        }

        [HttpPost("EditDepartment")]
        public IActionResult EditDepartment(DepartmentEditModel departmentDetails)
        {
            var existingDepartment = _dbContext.Departments.FirstOrDefault(d => d.Id == departmentDetails.Id);

            if (existingDepartment == null)
            {
                return NotFound(new { message = "Department not Found" });
            }

            if (_dbContext.Departments.Any(d => d.Name.ToUpper() == departmentDetails.Name.ToUpper() && d.Id != departmentDetails.Id))
            {
                return BadRequest(new
                {
                    message = "Department with this Name already Exist"
                });
            }
            try
            {
                existingDepartment.Name = departmentDetails.Name;
                _dbContext.SaveChanges();
                return Ok(existingDepartment);
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
        [HttpPost("DeleteDepartment")]
        public IActionResult DeleteDepartment([FromBody] int id)
        {
            var existingDepartment = _dbContext.Departments.FirstOrDefault(d => d.Id == id);

            if (existingDepartment == null)
            {
                return NotFound(new { message = "Department not Found" });
            }
            if (_dbContext.Employees.Any(e => e.Department.Id == existingDepartment.Id) || _dbContext.Designations.Any(d => d.Department.Id == existingDepartment.Id))
            {
                return BadRequest(new { message = $"{existingDepartment.Name} is in use and it can't be delete." });
            }


            try
            {
                _dbContext.Departments.Remove(existingDepartment);
                _dbContext.SaveChanges();
                return Ok(new {message = $"{existingDepartment.Name} has been deleted."});
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
