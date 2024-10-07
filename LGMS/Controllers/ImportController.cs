using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ExcelImportService _excelService;
        private readonly AttendanceRecordService _attendanceRecordService;
        LgmsDbContext _dbContext;



        public ImportController(AttendanceRecordService attendanceRecordService, ExcelImportService excelService, LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _excelService = excelService;
            _attendanceRecordService = attendanceRecordService;
        }

        [HttpPost("UploadEquipments")]
        public async Task<IActionResult> UploadEquipments(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Invalid file." });

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var equipments = _excelService.ParseEquipmentExcelFile(stream);

                    return Ok(new { message = $"{equipments.Count} records processed successfully." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new{message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("UploadAttendanceRecords")]
        public async Task<IActionResult> UploadAttendanceRecords(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Invalid file." });

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var attendanceRecords = _excelService.ParseAttendanceRecordExcelFile(stream);

                    return Ok(new { message = $"{attendanceRecords.Count} records processed successfully." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("UploadRawAttendanceRecords")]
        public async Task<IActionResult> UploadRawAttendanceRecords(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "File not selected." });
            }

            try
            {
                List<AttendanceRecord> attendanceRecords = new List<AttendanceRecord>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var excelAttendanceId = worksheet.Cells[row, 2].Value?.ToString();
                            var existingAttendanceId = _dbContext.AttendanceIds.FirstOrDefault(a => a.MachineName == excelAttendanceId);

                            if (existingAttendanceId == null)
                            {
                                continue;
                            }

                            var date = DateTime.Parse(worksheet.Cells[row, 3].Value?.ToString());

                            var existingRecord = _dbContext.AttendanceRecords
                                .FirstOrDefault(r => r.AttendanceId == existingAttendanceId && r.Date == date);

                            if (existingRecord != null)
                            {
                                continue;
                            }

                            var attendanceRecord = new AttendanceRecord
                            {
                                AttendanceId = existingAttendanceId,
                                Date = date,
                                CheckIns = worksheet.Cells[row, 4].Value?.ToString(),
                                CheckOuts = worksheet.Cells[row, 5].Value?.ToString(),
                                RequiredTime = TimeSpan.Parse(worksheet.Cells[row, 6].Value?.ToString()),
                                ActualTime = TimeSpan.Parse(worksheet.Cells[row, 7].Value?.ToString())
                            };

                            attendanceRecord.Status = _attendanceRecordService.CalculateStatus(
                                attendanceRecord.Date,
                                attendanceRecord.RequiredTime.ToString(),
                                attendanceRecord.ActualTime.ToString(),
                                "00:00:00",
                                "00:00:00",
                                attendanceRecord.CheckIns
                            );
                            attendanceRecord.OverHours = _attendanceRecordService.CalculateOverHours(attendanceRecord.RequiredTime, attendanceRecord.ActualTime);
                            attendanceRecord.UnderHours = _attendanceRecordService.CalculateUnderHours(attendanceRecord.RequiredTime, attendanceRecord.ActualTime, attendanceRecord.Status.Title);
                            attendanceRecord.IsRecordOk = true;

                            attendanceRecords.Add(attendanceRecord);
                        }
                    }
                }

                _attendanceRecordService.SaveAttendanceRecords(attendanceRecords);

                return Ok(new {message = "File processed and attendance records saved successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
