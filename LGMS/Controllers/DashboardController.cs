using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        public LgmsDbContext _dbContext;
        private readonly AttendanceReportService _reportService;

        public DashboardController(LgmsDbContext dbContext, AttendanceReportService reportService)
        {
            _dbContext = dbContext;
            _reportService = reportService;
        }

        [HttpGet("GetReportOfYear")]
        public IActionResult GetReportOfYear(string number, DateTime startDate, DateTime endDate)
        {
            var employee = _dbContext.Employees
                .Include(e => e.AttendanceId)
                .SingleOrDefault(e => e.EmployeeNumber.ToLower() == number.ToLower());

            if (employee == null)
            {
                return BadRequest(new { message = $"Employee with this number {number} not found." });
            }

            AttendanceId attendanceId = employee.AttendanceId;

            var attendanceRecords = _dbContext.AttendanceRecords
                .Include(a => a.AttendanceId)
                .Include(a => a.Status)
                .Where(e => e.AttendanceId == attendanceId && e.Date >= startDate && e.Date <= endDate)
                .ToList();

            AttendanceReportDTO report = _reportService.GenerateReport(employee.Name, attendanceRecords);

            return Ok(report);
        }


        [HttpGet("GetAllowancesSecureThisYear")]
        public IActionResult GetAllowancesSecureThisYear(string number, DateTime startDate, DateTime endDate)
        {
            var employee = _dbContext.Employees
               .Include(e => e.AttendanceId)
               .Include(e => e.Department)
               .Include(e => e.Designation)
               .SingleOrDefault(e => e.EmployeeNumber.ToLower() == number.ToLower());
            int allowancesCount = 0;
            var slips = _dbContext.SalarySlips
                .Include(s => s.Employee)
                .Where(s => s.Employee.Name == employee.Name && 
                       s.PayPeriod.Value >= startDate && s.PayPeriod.Value <= endDate &&
                       s.Paid == true)
                .ToList();
            foreach(var slip in slips)
            {
                if (slip.OnTimeAllowance) allowancesCount++;
                if (slip.AttendanceAllowance) allowancesCount++;
                if (slip.PerformanceAllowance == true) allowancesCount++;
            }
            return Ok(allowancesCount);
        }

        [HttpGet("GetEmployeeByNumber")]
        public IActionResult GetEmployeeByNumber(string number)
        {
            var employee = _dbContext.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Status)
                .Include(e => e.AttendanceId)
                .Include(e => e.Equipments)
                .ThenInclude(eq => eq.Type)
                .Include(eq => eq.Equipments)
                .ThenInclude(eq => eq.Manufacturer)
                .SingleOrDefault(e => e.EmployeeNumber.ToLower() == number.ToLower());
            if (employee == null)
            {
                return BadRequest(new { message = $"Employee with this number {number} not found." });
            }
            return Ok(employee);
        }

        [HttpGet("GetAttendanceData")]
        public IActionResult GetAttendanceData(string number, int month, int year)
        {
            var employee = _dbContext.Employees
                .Include(e => e.AttendanceId)
                .SingleOrDefault(e => e.EmployeeNumber.ToLower() == number.ToLower());

            if (employee == null)
            {
                return BadRequest(new { message = $"Employee with this number {number} not found." });
            }

            AttendanceId attendanceId = employee.AttendanceId;

            var attendanceRecords = _dbContext.AttendanceRecords
                .Include(a => a.AttendanceId)
                .Include(a => a.Status)
                .Where(e => e.AttendanceId == attendanceId && e.Date.Month == month && e.Date.Year == year)
                .ToList();
            var slip = _dbContext.SalarySlips
                .Include(s => s.Employee)
                .SingleOrDefault(s => s.Employee.Name == employee.Name && s.PayPeriod.Value.Month == month && s.PayPeriod.Value.Year == year && s.Paid == true);
            AttendanceReportDTO? report;
            if (attendanceRecords == null || attendanceRecords.Count == 0)
            {
                report = null;
            }
            else
            {
                report = _reportService.GenerateReport(employee.Name, attendanceRecords);
            }


            return Ok(new
            {
                report,
                slip,
                records = attendanceRecords
            });
        }
    }
}
