using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
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
        LgmsDbContext _dbContext;
        PagedData<Employee> _pagedData;
        public EmployeeController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Employee>();
        }
        [HttpPost("GetEmployees")]
        public ActionResult GetEmployees(EmployeesSearchModel employeeSearchModel)
        {
            if (employeeSearchModel == null) return BadRequest("Invalid search criteria");

            var employees = new List<Employee>();

            try
            {
                employees = _dbContext.Employees
                                      .Include(e => e.Status)
                                      .Include(e => e.Department)
                                      .Include(e => e.Designation)
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (!employees.Any()) return NotFound("Employees Not Found");

            if (!string.IsNullOrEmpty(employeeSearchModel.SearchDetails.SearchTerm))
            {
                employees = employees.Where(e =>
                    e.Name.ToUpper().Contains(employeeSearchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Designation.Title.ToUpper().Contains(employeeSearchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }

            if (!string.IsNullOrEmpty(employeeSearchModel.SortDetails.SortColumn) &&
                employeeSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (employeeSearchModel.SortDetails.SortColumn)
                {
                    case "employeeNumber":
                        employees = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    employees.OrderBy(e => e.EmployeeNumber).ToList() :
                                    employees.OrderByDescending(e => e.EmployeeNumber).ToList();
                        break;
                    case "employeeName":
                        employees = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    employees.OrderBy(e => e.Name).ToList() :
                                    employees.OrderByDescending(e => e.Name).ToList();
                        break;
                    case "attendanceId":
                        employees = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    employees.OrderBy(e => e.AttendanceId).ToList() :
                                    employees.OrderByDescending(e => e.AttendanceId).ToList();
                        break;
                    case "basicSalary":
                        employees = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    employees.OrderBy(e => e.BasicSalary).ToList() :
                                    employees.OrderByDescending(e => e.BasicSalary).ToList();
                        break;
                    case "status":
                        employees = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    employees.OrderBy(e => e.Status.Title).ToList() :
                                    employees.OrderByDescending(e => e.Status.Title).ToList();
                        break;
                    default:
                        employees = employeeSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    employees.OrderBy(e => e.Name).ToList() :
                                    employees.OrderByDescending(e => e.Name).ToList();
                        break;
                }
            }
            else
            {
                employees = employees.OrderByDescending(e => e.Name).ToList();
            }

            var pagedEmployeesResult = _pagedData.GetPagedData(
                employees,
                (PagedDataRequestModel)employeeSearchModel.PaginationDetails
            );

            return Ok(pagedEmployeesResult);
        }



        [HttpGet("GetEmployee")]
        public ActionResult GetEmployee(int employeeId)
        {
            var employee = _dbContext.Employees
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
            var employee = _dbContext.Employees
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
            if (_dbContext.Employees.FirstOrDefault(e => e.Name.ToUpper() == employeeDetails.EmployeeName.ToUpper()) != null) 
            {
                return BadRequest("Employee with this Name already Exist");
            }
            try
            {
                Employee employee = new Employee()
                {
                    AttendanceId = employeeDetails.AttendanceId,
                    Name = employeeDetails.EmployeeName,
                    EmployeeNumber = string.Format("{0}{1}", "EMP", DateTime.Now.ToString("yyMMddHHmmss")),
                    BirthDate = employeeDetails.BirthDate,
                    Department = employeeDetails.Department.Id == 0 ? 
                                 employeeDetails.Department :
                                 _dbContext.Departments.Single(d => d.Id == employeeDetails.Department.Id),
                    Designation = employeeDetails.Designation.Id == 0 ?
                                 employeeDetails.Designation :
                                 _dbContext.Designations.Single(d => d.Id == employeeDetails.Designation.Id),
                    JoiningDate = employeeDetails.JoiningDate,
                    BasicSalary = employeeDetails.BasicSalary,
                    AgreementExpiration = employeeDetails.AgreementExpiration,
                    Status = employeeDetails.Status.Id == 0 ?
                                 employeeDetails.Status :
                                 _dbContext.EmployeeStatus.Single(d => d.Id == employeeDetails.Status.Id)
                };
                _dbContext.Employees.Add(employee);
                _dbContext.SaveChanges();
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

        [HttpPost("EditEmployee")]
        public ActionResult EditEmployee(EmployeeEditModel employeeDetails)
        {
            var existingEmployee = _dbContext.Employees.FirstOrDefault(e => e.Id == employeeDetails.Id);

            if (existingEmployee == null)
            {
                return NotFound("Employee not found");
            }

            if (_dbContext.Employees.Any(e => e.Name.ToUpper() == employeeDetails.EmployeeName.ToUpper() && e.Id != employeeDetails.Id))
            {
                return BadRequest("Another employee with this name already exists");
            }

            try
            {
                existingEmployee.Name = employeeDetails.EmployeeName;
                existingEmployee.AttendanceId = employeeDetails.AttendanceId;
                existingEmployee.BirthDate = employeeDetails.BirthDate;
                existingEmployee.Department = employeeDetails.Department.Id == 0 ?
                                              employeeDetails.Department :
                                              _dbContext.Departments.Single(d => d.Id == employeeDetails.Department.Id);
                existingEmployee.Designation = employeeDetails.Designation.Id == 0 ?
                                               employeeDetails.Designation :
                                               _dbContext.Designations.Single(d => d.Id == employeeDetails.Designation.Id);
                existingEmployee.JoiningDate = employeeDetails.JoiningDate;
                existingEmployee.BasicSalary = employeeDetails.BasicSalary;
                existingEmployee.AgreementExpiration = employeeDetails.AgreementExpiration;
                existingEmployee.Status = employeeDetails.Status.Id == 0 ?
                                          employeeDetails.Status :
                                          _dbContext.EmployeeStatus.Single(d => d.Id == employeeDetails.Status.Id);

                _dbContext.SaveChanges();

                return Ok(existingEmployee);
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

        [HttpPost("DeleteEmployee")]
        public IActionResult DeleteEmployee([FromBody]int EmployeeId)
        {
            try
            {
                Employee? employee = _dbContext.Employees.FirstOrDefault(e => e.Id == EmployeeId);
                EmployeeStatus? employeeStatus =
                    _dbContext.EmployeeStatus.FirstOrDefault(s => s.Title.ToUpper() == "DELETED");
                if (employee == null) return BadRequest("Employee Id is not correct");
                if (employeeStatus == null) return BadRequest("Deleted Status Not Found");

                employee.Status = employeeStatus;
                _dbContext.Employees.Update(employee);
                _dbContext.SaveChanges();

                return Ok(employee);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("GetIncomingBirthdays")]
        public ActionResult GetIncomingBirthDays()
        {
            DateTime today = DateTime.Today;

            var employees = _dbContext.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Status)
                .Where(e => e.Status.Title == "Active").ToList();

            int currentYear = today.Year;

            var upcomingBirthdays = employees
                .Select(e => new
                {
                    Employee = e,
                    BirthdayThisYear = new DateTime(currentYear, e.BirthDate.Month, e.BirthDate.Day),
                    BirthdayNextYear = new DateTime(currentYear + 1, e.BirthDate.Month, e.BirthDate.Day)
                })
                .Where(e => e.BirthdayThisYear >= today || e.BirthdayNextYear >= today) 
                .OrderBy(e => e.BirthdayThisYear >= today ? e.BirthdayThisYear : e.BirthdayNextYear) 
                .Take(3)
                .Select(e => e.Employee) 
                .ToList();

            return Ok(upcomingBirthdays);
        }


        [HttpGet("GetIncomingAgreementExpiration")]
        public ActionResult GetIncomingAgreementExpiration()
        {
            var Employees = _dbContext.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Status)
                .Where(e => e.Status.Title == "Active")
                .OrderBy(e => e.AgreementExpiration)
                .Take(3)
                .ToList();
            return Ok(Employees);
        }
    }
}
