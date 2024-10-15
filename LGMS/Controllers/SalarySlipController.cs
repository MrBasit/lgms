using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using LGMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using System.IO.Compression;
using QuestPDF.Helpers;
using System.Globalization;

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
                var query = _dbContext.AttendanceRecords
                    .Include(e => e.AttendanceId)
                    .Include(e => e.Status)
                    .AsQueryable();

                if (searchModel.Year > 0)
                {
                    query = query.Where(ar => ar.Date.Year == searchModel.Year);
                }
                else
                {
                    return BadRequest(new { message = "Year is required." });
                }

                if (searchModel.Month > 0)
                {
                    query = query.Where(ar => ar.Date.Month == searchModel.Month);
                }
                else
                {
                    return BadRequest(new { message = "Month is required." });
                }

                if (searchModel.MachineNames?.Any() == true)
                {
                    var lowerCaseMachineNames = searchModel.MachineNames.Select(name => name.ToLower()).ToList();
                    query = query.Where(ar => lowerCaseMachineNames.Contains(ar.AttendanceId.MachineName.ToLower()));
                }

                var attendanceRecords = query.ToList();

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
                        .Include(e => e.Designation)
                        .Where(e => e.AttendanceId.MachineName.ToUpper() == report.Name)
                        .Select(e => new ConfigurationDTO()
                        {
                            Name = e.Name,
                            Salary = e.BasicSalary,
                            Designation = e.Designation.Title,
                        })
                        .FirstOrDefault();

                    var salarySlip = salarySlipService.GenerateSalarySlip(report, configuration);
                    salarySlips.Add(salarySlip);
                }

                if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
                {
                    salarySlips = salarySlips.Where(e =>
                        e.Name.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())).ToList();
                }

                if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                    searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
                {
                    switch (searchModel.SortDetails.SortColumn)
                    {
                        case "name":
                            salarySlips = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                salarySlips.OrderBy(e => e.Name).ToList() :
                                salarySlips.OrderByDescending(e => e.Name).ToList();
                            break;
                        case "deductions":
                            salarySlips = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                salarySlips.OrderBy(e => e.Deductions).ToList() :
                                salarySlips.OrderByDescending(e => e.Deductions).ToList();
                            break;
                        case "overtime":
                            salarySlips = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                salarySlips.OrderBy(e => e.Overtime).ToList() :
                                salarySlips.OrderByDescending(e => e.Overtime).ToList();
                            break;
                        default:
                            salarySlips = salarySlips.OrderBy(e => e.Name).ToList();
                            break;
                    }
                }
                else
                {
                    salarySlips = salarySlips.OrderBy(e => e.Name).ToList();
                }

                var pagedSalarySlipResult = _pagedData.GetPagedData(
                    salarySlips,
                    (PagedDataRequestModel)searchModel.PaginationDetails
                );

                return Ok(pagedSalarySlipResult);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("GenerateSalarySlips")]
        public IActionResult GenerateSalarySlips([FromBody] List<SalarySlipDTO> salarySlips)
        {
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "company-logo.png");
            using (var zipStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var slip in salarySlips)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            var dbSlip = new SalarySlip()
                            {
                                Name = slip.Name,
                                Designation = slip.Designation,
                                UpdatedDate = DateTime.Now,
                                Salary = slip.Salary,
                                Deductions = slip.Deductions,
                                OnTimeAllowance = slip.OnTimeAllowance,
                                PerformanceAllowance = slip.PerformanceAllowance,
                                AttendanceAllowance = slip.AttendanceAllowance,
                                Overtime = slip.Overtime,
                                Comission = slip.Comission,
                                Total = slip.Total
                            };
                            _dbContext.SalarySlips.Add(dbSlip);
                            var document = CreateSalarySlipPdf(slip, logoPath);
                            document.GeneratePdf(memoryStream);

                            var zipEntry = archive.CreateEntry($"{slip.Name}_SalarySlip.pdf", CompressionLevel.Optimal);
                            using (var entryStream = zipEntry.Open())
                            {
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                memoryStream.CopyTo(entryStream);
                            }
                        }
                        _dbContext.SaveChanges();
                    }
                }

                return File(zipStream.ToArray(), "application/zip", "SalarySlips.zip");
            }
        }

        private IDocument CreateSalarySlipPdf(SalarySlipDTO slip, string logoPath)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().AlignCenter().Height(80).Image(logoPath, ImageScaling.FitHeight); 

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().AlignCenter().Text($"SALARY SLIP OF {DateTime.Now.AddMonths(-1):MMMM, yyyy}").Bold().FontSize(14).FontColor(Colors.Black);

                        col.Item().PaddingTop(10).PaddingBottom(5).Text($"NAME: {slip.Name}").Bold();
                        col.Item().Text($"DESIGNATION: {slip.Designation}").Bold();

                        col.Item().AlignCenter().PaddingTop(20).Text("EARNINGS").Bold().FontSize(14);

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(); 
                                columns.RelativeColumn(); 
                            });

                            table.Cell().Border(1).Padding(5).Text("Salary").Bold();
                            table.Cell().Border(1).Padding(5).Text(slip.Salary.ToString("C", new CultureInfo("ur-PK")));

                            table.Cell().Border(1).Padding(5).Text("Deductions").Bold();
                            table.Cell().Border(1).Padding(5).Text(slip.Deductions.ToString("C", new CultureInfo("ur-PK")));

                            table.Cell().Border(1).Padding(5).Text("On Time Allowance").Bold();
                            table.Cell().Border(1).Padding(5).Text(slip.OnTimeAllowance ? "Yes" : "No");

                            table.Cell().Border(1).Padding(5).Text("Attendance Allowance").Bold();
                            table.Cell().Border(1).Padding(5).Text(slip.AttendanceAllowance ? "Yes" : "No");

                            table.Cell().Border(1).Padding(5).Text("Performance Allowance").Bold();
                            table.Cell().Border(1).Padding(5).Text(slip.PerformanceAllowance.HasValue && slip.PerformanceAllowance.Value ? "Yes" : "No");

                            table.Cell().Border(1).Padding(5).Text("Overtime").Bold();
                            table.Cell().Border(1).Padding(5).Text(slip.Overtime.ToString("C", new CultureInfo("ur-PK")));

                            table.Cell().Border(1).Padding(5).Text("Commission").Bold();
                            table.Cell().Border(1).Padding(5).Text(slip.Comission.HasValue ? slip.Comission.Value.ToString("C", new CultureInfo("ur-PK")) : "N/A");

                            table.Cell().Border(1).Padding(5).Text("Total").Bold();
                            table.Cell().Border(1).Padding(5).Text(slip.Total.ToString("C", new CultureInfo("ur-PK"))).Bold();
                        });

                        col.Item().PaddingTop(10).Text("DEDUCTION APPLIED:").Bold();
                        col.Item().Text(slip.Deductions > 0 ? "TRUE" : "FALSE");

                        col.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("DIRECTOR ________________________");
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });
        }

    }
}
