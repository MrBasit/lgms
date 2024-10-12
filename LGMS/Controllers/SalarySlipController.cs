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
    public class SalarySlipController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<SalarySlipDTO> _pagedData;
        private readonly AttendanceReportService _reportService;
        private readonly SalarySlipService _salarySlipService;


        public SalarySlipController(LgmsDbContext dbContext, AttendanceReportService reportService, SalarySlipService salarySlipService)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<SalarySlipDTO>();
            _reportService = reportService;
            _salarySlipService = salarySlipService;
        }

        [HttpPost("GetSalarySlips")]
        public IActionResult GetSalarySlips(SalarySlipSearchModel searchModel)
        {
            if (searchModel == null)
                return BadRequest(new { message = "Invalid search criteria" });

            try
            {
                var attendanceRecords = _dbContext.AttendanceRecords
                    .Include(e => e.AttendanceId)
                    .Include(e => e.Status)
                    .ToList();

                if (searchModel.Year > 0)
                {
                    attendanceRecords = attendanceRecords
                        .Where(ar => ar.Date.Year == searchModel.Year).ToList();
                }
                else
                {
                    return BadRequest(new { message = "Year is required." });
                }

                if (searchModel.Month > 0)
                {
                    attendanceRecords = attendanceRecords
                        .Where(ar => ar.Date.Month == searchModel.Month).ToList();
                }
                else
                {
                    return BadRequest(new { message = "Month is required." });
                }

                if (searchModel.MachineNames?.Any() == true)
                {
                    attendanceRecords = attendanceRecords
                        .Where(ar => searchModel.MachineNames
                            .Any(name => ar.AttendanceId.MachineName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                var attendanceReports = new List<AttendanceReportDTO>();
                var reportService = _reportService;

                foreach (var name in searchModel.MachineNames)
                {
                    var employeeRecords = attendanceRecords
                        .Where(x => x.AttendanceId.MachineName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (employeeRecords.Any())
                    {
                        var report = reportService.GenerateReport(name, employeeRecords);
                        attendanceReports.Add(report);
                    }
                }

                var salarySlips = new List<SalarySlipDTO>();
                var salarySlipService = _salarySlipService;
                foreach (var report in attendanceReports)
                {
                    var configuration = _dbContext.Employees
                        .Include(e => e.AttendanceId)
                        .Where(e => e.AttendanceId.MachineName.ToUpper() == report.Name)
                        .Select(e => new ConfigurationDTO()
                        {
                            Name = e.Name,
                            Salary = e.BasicSalary
                        })
                        .FirstOrDefault();
                    var salarySlip = salarySlipService.GenerateSalarySlip(report, configuration);
                    salarySlips.Add(salarySlip);
                }

                return Ok(salarySlips);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
