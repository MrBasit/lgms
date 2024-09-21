using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        LgmsDbContext _dbContex;
        public EmployeeController(LgmsDbContext dbContext)
        {
            _dbContex = dbContext;
        }
        [HttpPost("GetEmployees")]
        public ActionResult GetEmployees(EmployeesSearchModel employeeSearchModel)
        {
            var employeesQuery = _dbContex.Employees
                                 .Include(e=>e.Status).Include(e => e.Department).Include(e => e.Designation)
                                 .AsQueryable();
            if (!string.IsNullOrEmpty(employeeSearchModel.SearchDetails.SearchTerm)) 
            {
                employeesQuery = employeesQuery.Where(e => e.Name.ToUpper().Contains(employeeSearchModel.SearchDetails.SearchTerm.ToUpper()));
            }
            if (!string.IsNullOrEmpty(employeeSearchModel.SortDetails.SortColumn) && employeeSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (employeeSearchModel.SortDetails.SortColumn)
                {
                    case "employeeNumber":
                        employeesQuery = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                         employeesQuery.OrderBy(e => e.EmployeeNumber) :
                                         employeesQuery.OrderByDescending(e => e.EmployeeNumber);
                        break;
                    case "employeeName":
                        employeesQuery = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                         employeesQuery.OrderBy(e => e.Name) :
                                         employeesQuery.OrderByDescending(e => e.Name);
                        break;
                    case "attendaceId":
                        employeesQuery = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                         employeesQuery.OrderBy(e => e.AttandanceId) :
                                         employeesQuery.OrderByDescending(e => e.AttandanceId);
                        break;
                    case "basicSalary":
                        employeesQuery = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                         employeesQuery.OrderBy(e => e.BasicSalary) :
                                         employeesQuery.OrderByDescending(e => e.BasicSalary);
                        break;
                    case "status":
                        employeesQuery = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                         employeesQuery.OrderBy(e => e.Status.Title) :
                                         employeesQuery.OrderByDescending(e => e.Status.Title);
                        break;
                    default:
                        employeesQuery = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                         employeesQuery.OrderBy(e => e.Name) :
                                         employeesQuery.OrderByDescending(e => e.Name);
                        break;

                }
            } else{
                employeesQuery = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                         employeesQuery.OrderBy(e => e.Name) :
                                         employeesQuery.OrderByDescending(e => e.Name);
            }

            employeesQuery = employeesQuery.Skip((employeeSearchModel.PaginationDetails.PageNumber - 1) * employeeSearchModel.PaginationDetails.PageSize)
            .Take(employeeSearchModel.PaginationDetails.PageSize);

            return Ok(employeesQuery.ToList());
        }

        [HttpGet("GetEmployee")]
        public ActionResult GetEmployee(int employeeId)
        {
            var employee = _dbContex.Employees
                            .Include(_ => _.Department)
                            .Include(_ => _.Designation)
                            .Include(_ => _.Status)
                            .SingleOrDefault(e => e.Id == employeeId);
            if (employee == null) return BadRequest(string.Format("Employee with id {0} doesn't exist", employeeId));
            return Ok(employee);
        }

        [HttpGet("GetEmployeeByName")]
        public ActionResult GetEmployeeByName(string employeeName)
        {
            var employee = _dbContex.Employees
                            .Include(_ => _.Department)
                            .Include(_ => _.Designation)
                            .Include(_ => _.Status)
                            .Where(e => e.Name == employeeName)
                            .ToList();

            if (employee == null) return BadRequest(string.Format("Employee with employeeName {0} doesn't exist", employeeName));
            if (employee.Count() > 1) return BadRequest(string.Format("Multiple employees found with employee name {0}", employeeName));
            return Ok(employee);
        }

        [HttpPost("AddEmployee")]
        public ActionResult AddEmployee(EmployeeAddModel employeeDetails)
        {
            if (_dbContex.Employees.FirstOrDefault(e => e.Name.ToUpper() == employeeDetails.EmployeeName.ToUpper()) != null) 
            {
                return BadRequest("Employee with this Name already Exist");
            }
            try
            {
                Employee employee = new Employee()
                {
                    AttandanceId = employeeDetails.AttandanceId,
                    Name = employeeDetails.EmployeeName,
                    EmployeeNumber = string.Format("{0}{1}", "EMP", DateTime.Now.ToString("yyMMddHHmmss")),
                    BirthDate = employeeDetails.BirthDate,
                    Department = employeeDetails.Department.Id == 0 ? 
                                 employeeDetails.Department : 
                                 _dbContex.Departments.Single(d => d.Id == employeeDetails.Department.Id),
                    Designation = employeeDetails.Designation.Id == 0 ?
                                 employeeDetails.Designation :
                                 _dbContex.Designations.Single(d => d.Id == employeeDetails.Designation.Id),
                    JoiningDate = employeeDetails.JoiningDate,
                    BasicSalary = employeeDetails.BasicSalary,
                    AgreementExpiration = employeeDetails.AgreementExpiration,
                    Status = employeeDetails.Status.Id == 0 ?
                                 employeeDetails.Status :
                                 _dbContex.EmployeeStatus.Single(d => d.Id == employeeDetails.Status.Id)
                };
                _dbContex.Employees.Add(employee);
                _dbContex.SaveChanges();
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message=ex.Message,
                    innerMessage = ex.InnerException!=null?ex.InnerException.Message:""
                });
            }
        }

        [HttpPost("GetIncomingBirthdays")]
        public ActionResult GetIncommingBirthDays()
        {
            var Employees = _dbContex.Employees.Where(
                e => e.Status.Title == "Active"
                && e.BirthDate.Month >= DateTime.Now.Month
                && e.BirthDate.Day >= DateTime.Now.Day
                )
                .OrderBy(e=>e.BirthDate)
                .Take(3)
                .ToList();
            return Ok(Employees);
        }

        [HttpPost("GetIncomingAgreementExpiration")]
        public ActionResult GetIncomingAgreementExpiration()
        {
            var Employees = _dbContex.Employees.Where(e => e.Status.Title == "Active")
                            .OrderBy(e => e.AgreementExpiration)
                            .Take(3)
                            .ToList();
            return Ok(Employees);
        }
    }
}
