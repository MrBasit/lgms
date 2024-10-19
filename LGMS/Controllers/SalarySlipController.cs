﻿using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using LGMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using QuestPDF.Fluent;


namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalarySlipController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<SalarySlipDTO> _dtoData;
        PagedData<SalarySlip> _salaryData;
        private readonly AttendanceReportService _reportService;
        private readonly SalarySlipService _salarySlipService;
        private readonly SalarySlipPDFService _pdfService;


        public SalarySlipController(LgmsDbContext dbContext, AttendanceReportService reportService, SalarySlipService salarySlipService, SalarySlipPDFService pdfService)
        {
            _dbContext = dbContext;
            _dtoData = new PagedData<SalarySlipDTO>();
            _salaryData = new PagedData<SalarySlip>();
            _reportService = reportService;
            _salarySlipService = salarySlipService;
            _pdfService = pdfService;
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
                        .Include(e => e.Department)
                        .Where(e => e.AttendanceId.MachineName.ToUpper() == report.Name)
                        .Select(e => new ConfigurationDTO()
                        {
                            Name = e.Name,
                            Salary = e.BasicSalary,
                            Designation = e.Designation.Title,
                            Department = e.Department.Name
                        })
                        .FirstOrDefault();

                    var salarySlip = salarySlipService.GenerateSalarySlip(report, configuration, searchModel.Year, searchModel.Month);
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

                var pagedSalarySlipResult = _dtoData.GetPagedData(
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

        [HttpPost("ViewSalarySlips")]
        public IActionResult ViewSalarySlips(SalarySlipSearchModel searchModel)
        {
            if (searchModel == null)
                return BadRequest(new { message = "Invalid search criteria" });

            try
            {
                var query = _dbContext.SalarySlips
                    .AsQueryable();

                if (searchModel.Year <= 0)
                    return BadRequest(new { message = "Year is required." });

                if (searchModel.Month <= 0)
                    return BadRequest(new { message = "Month is required." });

                query = query.Where(ar => ar.PayPeriod.HasValue &&
                          ar.PayPeriod.Value.Year == searchModel.Year &&
                          ar.PayPeriod.Value.Month == searchModel.Month);

                if (searchModel.MachineNames?.Any() == true)
                {
                    var employeeNames = _dbContext.Employees
                        .Where(e => searchModel.MachineNames.Contains(e.AttendanceId.MachineName))
                        .Select(e => e.Name.ToLower())
                        .ToList();

                    query = query.Where(ar => employeeNames.Contains(ar.Name.ToLower()));
                }

                var salarySlips = query.ToList();

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

                var pagedSalarySlipResult = _salaryData.GetPagedData(
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
        public IActionResult GenerateSalarySlips([FromBody] List<SalarySlipDTO> salarySlips, int month, int year)
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
                                Department = slip.Department,
                                GenratedDate = slip.GenratedDate,
                                PayPeriod = slip.PayPeriod,
                                Salary = slip.Salary,
                                Deductions = slip.Deductions,
                                OnTimeAllowance = slip.OnTimeAllowance,
                                PerformanceAllowance = slip.PerformanceAllowance,
                                AttendanceAllowance = slip.AttendanceAllowance,
                                Overtime = slip.Overtime,
                                SecurityDeposit = slip.SecurityDeposit,
                                IncomeTax = slip.IncomeTax,
                                Loan = slip.Loan,
                                Comission = slip.Comission,
                                Total = slip.Total
                            };
                            _dbContext.SalarySlips.Add(dbSlip);
                            var document = _pdfService.CreateSalarySlipPdf(slip, logoPath);
                            document.GeneratePdf(memoryStream);

                            var zipEntry = archive.CreateEntry($"{slip.Name}_SalarySlip.pdf", CompressionLevel.Optimal);
                            using (var entryStream = zipEntry.Open())
                            {
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                memoryStream.CopyTo(entryStream);
                            }
                        }
                    }
                    _dbContext.SaveChanges();
                }
                return File(zipStream.ToArray(), "application/zip", "SalarySlips.zip");
            }
        }
    }
}