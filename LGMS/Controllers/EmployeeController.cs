using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace LGMS.Controllers
{
    [Authorize(Roles = "Admin")]
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
        [HttpPost("GetEmployeesWithFilters")]
        public IActionResult GetEmployeesWithFilters(EmployeesSearchModel employeeSearchModel)
        {
            if (employeeSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var employees = new List<Employee>();

            try
            {
                employees = _dbContext.Employees
                                      .Include(e => e.Status)
                                      .Include(e => e.Department)
                                      .Include(e => e.Designation)
                                      .Include(e => e.AttendanceId)
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
            if (!employees.Any()) return NotFound(new { message = "No employees are there" });


            var employeesWithIncludedStatuses = new List<Employee>();

            foreach (var status in employeeSearchModel.Statuses)
            {
                employeesWithIncludedStatuses.AddRange(employees.Where(x => x.Status.Id == status.Id).ToList());
            }

            employees = employeesWithIncludedStatuses;

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
                                    employees.OrderBy(e => e.AttendanceId.Id).ToList() :
                                    employees.OrderByDescending(e => e.AttendanceId.Id).ToList();
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
                employees = employees.OrderBy(e => e.Name).ToList();
            }

            var pagedEmployeesResult = _pagedData.GetPagedData(
                employees,
                (PagedDataRequestModel)employeeSearchModel.PaginationDetails
            );

            return Ok(pagedEmployeesResult);
        }

        [Authorize(Roles = "Stores")]
        [HttpGet("GetEmployees")]
        public IActionResult GetEmployees()
        {
            var employees = _dbContext.Employees.Include(e => e.AttendanceId).Where(e => e.AttendanceId != null && e.Status.Title == "Active").OrderBy(e => e.Name).ToList();
            return Ok(employees);
        }

        [HttpGet("GetUserEmployees")]
        public IActionResult GetUserEmployees()
        {
            var employees = _dbContext.Employees.Include(e => e.IdentityUser).Where(e => e.IdentityUser != null).OrderBy(e => e.Name)
                .Select(v => new {
                    name = v.Name,
                    employeeNumber = v.EmployeeNumber,
                    userName = v.IdentityUser.UserName, 
                    userEmail = v.IdentityUser.Email
                });
            return Ok(employees);
        }

        [HttpGet("GetEmployeeById")]
        public IActionResult GetEmployeeById(int employeeId)
        {
            var employee = _dbContext.Employees
                            .Include(_ => _.Department)
                            .Include(_ => _.Designation)
                            .Include(_ => _.Status)
                            .Include(e => e.AttendanceId)
                            .Include(e => e.SecurityDeposits)
                            .Include(e => e.Loans)
                            .Include(e => e.Equipments).ThenInclude(eq => eq.Type)
                            .Include(e => e.IdentityUser)
                            .SingleOrDefault(e => e.Id == employeeId);
            if (employee == null) return BadRequest(new { message = string.Format("Employee with id {0} doesn't exist", employeeId) });
            return Ok(employee);
        }

        [HttpGet("GetEmployeeByName")]
        public IActionResult GetEmployeeByName(string employeeName)
        {
            var employee = _dbContext.Employees
                            .Include(_ => _.Department)
                            .Include(_ => _.Designation)
                            .Include(_ => _.Status)
                            .Where(e => e.Name == employeeName)
                            .ToList();

            if (employee == null) return BadRequest(new { message = string.Format("Employee with employeeName {0} doesn't exist", employeeName) });
            if (employee.Count() > 1) return BadRequest(new { message = string.Format("Multiple employees found with employee name {0}", employeeName) });
            return Ok(employee);
        }
        
        [HttpGet("GetEmployessIdAndName")]
        public IActionResult GetEmployeesIdAndName()
        {
            var employees = _dbContext.Employees
                .Select(e => new
                {
                    Id = e.Id,
                    Name = e.Name
                })
                .ToList();

            return Ok(employees);
        }

        [HttpPost("AddEmployee")]
        public IActionResult AddEmployee(EmployeeAddModel employeeDetails)
        {
            if (_dbContext.Employees.Any(e => e.Name.ToUpper() == employeeDetails.EmployeeName.ToUpper()))
            {
                return BadRequest(new { message = "Employee with this Name already exists." });
            }

            AttendanceId attendanceId = null;

            if (!string.IsNullOrEmpty(employeeDetails.AttendanceId))
            {
                var stringFromFrontend = employeeDetails.AttendanceId;
                var numberMatch = System.Text.RegularExpressions.Regex.Match(stringFromFrontend, @"\d+");

                if (!numberMatch.Success)
                {
                    return BadRequest(new { message = "No valid number found in the attendance ID string." });
                }

                var extractedNumber = int.Parse(numberMatch.Value);
                attendanceId = _dbContext.AttendanceIds.SingleOrDefault(a => a.MachineId == extractedNumber);

                if (attendanceId != null && _dbContext.Employees.Any(e => e.AttendanceId.Id == attendanceId.Id))
                {
                    return BadRequest(new { message = "This Attendance ID is already linked to another employee." });
                }
            }

            try
            {
                string employeeNumber = GenerateEmployeeNumber();

                var department = employeeDetails.Department.Id == 0
                    ? employeeDetails.Department
                    : _dbContext.Departments.Single(d => d.Id == employeeDetails.Department.Id);

                var designation = employeeDetails.Designation.Id == 0
                    ? employeeDetails.Designation
                    : _dbContext.Designations.Single(d => d.Id == employeeDetails.Designation.Id);
                if (_dbContext.Designations.Any(d => d.Title.ToUpper() == designation.Title.ToUpper() && d.Department.Id != department.Id))
                {
                    return BadRequest(new { message = $"This {designation.Title} already exists in another department." });
                }

                if (employeeDetails.Designation.Id == 0)
                {
                    designation.Department = department;
                }
                if (employeeDetails.Department.Id == 0)
                {
                    department = employeeDetails.Department;
                }

                Employee employee = new Employee()
                {
                    AttendanceId = attendanceId,
                    Name = employeeDetails.EmployeeName,
                    FatherName = employeeDetails.FatherName,
                    Email = employeeDetails.Email,
                    PhoneNumber = employeeDetails.PhoneNumber,
                    NIC = employeeDetails.NIC,
                    EmployeeNumber = employeeNumber,
                    BirthDate = employeeDetails.BirthDate,
                    Department = department,
                    Designation = designation,
                    JoiningDate = employeeDetails.JoiningDate,
                    BasicSalary = employeeDetails.BasicSalary,
                    AgreementExpiration = employeeDetails.AgreementExpiration,
                    Status = employeeDetails.Status.Id == 0
                        ? employeeDetails.Status
                        : _dbContext.EmployeeStatus.Single(d => d.Id == employeeDetails.Status.Id)
                };

                _dbContext.Employees.Add(employee);
                _dbContext.SaveChanges();
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }



        [HttpPost("EditEmployee")]
        public ActionResult EditEmployee(EmployeeEditModel employeeDetails)
        {
            var existingEmployee = _dbContext.Employees
                .Include(e => e.AttendanceId)
                .Include(e => e.Equipments)
                .FirstOrDefault(e => e.Id == employeeDetails.Id);

            if (existingEmployee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            if (_dbContext.Employees.Any(e => e.Name.ToUpper() == employeeDetails.EmployeeName.ToUpper() && e.Id != employeeDetails.Id))
            {
                return BadRequest(new { message = "Another employee with this name already exists" });
            }

            if (!string.IsNullOrEmpty(employeeDetails.AttendanceId))
            {
                var stringFromFrontend = employeeDetails.AttendanceId;
                var numberMatch = System.Text.RegularExpressions.Regex.Match(stringFromFrontend, @"\d+");
                if (!numberMatch.Success)
                {
                    return BadRequest(new { message = "No valid number found in the attendance ID string." });
                }

                var extractedNumber = int.Parse(numberMatch.Value);
                var attendanceId = _dbContext.AttendanceIds.SingleOrDefault(a => a.MachineId == extractedNumber);

                if (attendanceId != null && _dbContext.Employees.Any(e => e.AttendanceId.Id == attendanceId.Id && e.Id != employeeDetails.Id))
                {
                    return BadRequest(new { message = "Another employee with this Attendance ID already exists" });
                }

                existingEmployee.AttendanceId = attendanceId;
            }
            else
            {
                existingEmployee.AttendanceId = null;
            }

            try
            {
                existingEmployee.Name = employeeDetails.EmployeeName;
                existingEmployee.FatherName = employeeDetails.FatherName;
                existingEmployee.Email = employeeDetails.Email;
                existingEmployee.PhoneNumber = employeeDetails.PhoneNumber;
                existingEmployee.NIC = employeeDetails.NIC;
                existingEmployee.BirthDate = employeeDetails.BirthDate;
                existingEmployee.JoiningDate = employeeDetails.JoiningDate;
                existingEmployee.BasicSalary = employeeDetails.BasicSalary;
                existingEmployee.AgreementExpiration = employeeDetails.AgreementExpiration;

                var department = employeeDetails.Department.Id == 0
                    ? employeeDetails.Department
                    : _dbContext.Departments.Single(d => d.Id == employeeDetails.Department.Id);

                var designation = employeeDetails.Designation.Id == 0
                    ? employeeDetails.Designation
                    : _dbContext.Designations.Single(d => d.Id == employeeDetails.Designation.Id);
                if (_dbContext.Designations.Any(d => d.Title.ToUpper() == designation.Title.ToUpper() && d.Department.Id != department.Id))
                {
                    return BadRequest(new { message = $"This {designation.Title} already exists in another department." });
                }

                if (employeeDetails.Designation.Id == 0)
                {
                    designation.Department = department;
                }
                if (employeeDetails.Department.Id == 0)
                {
                    department = employeeDetails.Department;
                }

                existingEmployee.Department = department;
                existingEmployee.Designation = designation;

                existingEmployee.Status = employeeDetails.Status.Id == 0
                    ? employeeDetails.Status
                    : _dbContext.EmployeeStatus.Single(d => d.Id == employeeDetails.Status.Id);

                _dbContext.SaveChanges();

                return Ok(existingEmployee);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
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
                if (employee == null) return BadRequest(new { message = "Employee Id is not correct" });
                if (employeeStatus == null) return BadRequest(new { message = "Deleted Status Not Found" });

                employee.Status = employeeStatus;
                _dbContext.Employees.Update(employee);
                _dbContext.SaveChanges();

                return Ok(employee);

            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }


        [HttpGet("GetIncomingBirthdays")]
        public ActionResult GetIncomingBirthDays()
        {
            DateTime today = DateTime.Today;
            int currentYear = today.Year;

            var employees = _dbContext.Employees
                .Where(e => e.Status.Title == "Active")
                .ToList();

            var upcomingBirthdays = employees
                .Select(e => new
                {
                    Employee = e,
                    OriginalBirthDate = e.BirthDate,
                    BirthdayThisYear = new DateTime(currentYear, e.BirthDate.Month, e.BirthDate.Day),
                    BirthdayNextYear = new DateTime(currentYear + 1, e.BirthDate.Month, e.BirthDate.Day)
                })
                .Where(e => e.BirthdayThisYear >= today || e.BirthdayNextYear >= today)
                .OrderBy(e => e.BirthdayThisYear >= today ? e.BirthdayThisYear : e.BirthdayNextYear)
                .Select(e => $"{e.Employee.Name} - {e.OriginalBirthDate:MMM dd,yyyy}")
                .ToList();

            return Ok(upcomingBirthdays);
        }



        [HttpGet("GetIncomingAgreementExpiration")]
        public ActionResult GetIncomingAgreementExpiration()
        {
            var Employees = _dbContext.Employees
                .Where(e => e.Status.Title == "Active")
                .OrderBy(e => e.AgreementExpiration)
                .Select(e => $"{e.Name} - {e.AgreementExpiration:MMM dd,yyyy}");
            return Ok(Employees);
        }

        [HttpGet("GetDepartmentsWithEmployeeCount")]
        public ActionResult GetDepartmentsWithEmployeeCount()
        {
            var departmentsWithCount = _dbContext.Departments
                .Select(d => new
                {
                    DepartmentName = d.Name,
                    EmployeeCount = _dbContext.Employees.Where(e=> e.Status.Title =="Active").Count(e => e.Department.Id == d.Id) 
                })
                .OrderByDescending(d => d.EmployeeCount)
                .Select(d => $"{d.DepartmentName} - {d.EmployeeCount}"); 

            return Ok(departmentsWithCount);
        }


        private string GenerateEmployeeNumber()
        {
            var lastEmployee = _dbContext.Employees
                .OrderByDescending(c => c.EmployeeNumber)
                .FirstOrDefault();

            if (lastEmployee == null)
            {
                return "LGEM0001";
            }

            var lastEmployeeNumber = lastEmployee.EmployeeNumber;
            var numberPart = lastEmployeeNumber.Substring(4);
            var nextNumber = (int.Parse(numberPart) + 1).ToString();
            return "LGEM" + nextNumber.PadLeft(Math.Max(numberPart.Length, nextNumber.Length), '0');
        }

    }
}
