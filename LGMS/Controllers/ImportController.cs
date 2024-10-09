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

                        var machineIds = new HashSet<int>();
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var excelMachineIdValue = worksheet.Cells[row, 1].Value;
                            if (excelMachineIdValue != null && int.TryParse(excelMachineIdValue.ToString(), out int machineId))
                            {
                                machineIds.Add(machineId);
                            }
                        }

                        var existingAttendanceIds = _dbContext.AttendanceIds
                            .Where(a => machineIds.Contains(a.MachineId))
                            .ToList();

                        var existingRecords = _dbContext.AttendanceRecords
                            .Where(r => existingAttendanceIds.Select(a => a.Id).Contains(r.AttendanceId.Id))
                            .ToList();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var excelMachineIdValue = worksheet.Cells[row, 1].Value;
                            int machineId = Convert.ToInt32(excelMachineIdValue);

                            var existingAttendanceId = existingAttendanceIds.FirstOrDefault(a => a.MachineId == machineId);
                            if (existingAttendanceId == null)
                            {
                                continue;
                            }

                            var date = DateTime.Parse(worksheet.Cells[row, 3].Value?.ToString());

                            var existingRecord = existingRecords.FirstOrDefault(r => r.AttendanceId.Id == existingAttendanceId.Id && r.Date == date);
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
                                ActualTime = TimeSpan.Parse(worksheet.Cells[row, 7].Value?.ToString()),
                                TimeTable = worksheet.Cells[row, 8].Value?.ToString(),
                                LateIn = TimeSpan.Parse(worksheet.Cells[row, 12].Value?.ToString()),
                            };

                            attendanceRecord.Status = _attendanceRecordService.CalculateStatus(
                                attendanceRecord.Date,
                                attendanceRecord.RequiredTime.ToString(),
                                attendanceRecord.ActualTime.ToString(),
                                "00:00:00",
                                attendanceRecord.LateIn.ToString(),
                                attendanceRecord.CheckIns
                            );
                            attendanceRecord.OverHours = _attendanceRecordService.CalculateOverHours(attendanceRecord.RequiredTime, attendanceRecord.ActualTime);
                            attendanceRecord.UnderHours = _attendanceRecordService.CalculateUnderHours(attendanceRecord.RequiredTime, attendanceRecord.ActualTime, attendanceRecord.Status.Title);
                            attendanceRecord.IsRecordOk = true;

                            attendanceRecords.Add(attendanceRecord);
                        }
                    }
                }

                await _attendanceRecordService.SaveAttendanceRecordsAsync(attendanceRecords);

                return Ok(new { message = "File processed and attendance records saved successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
