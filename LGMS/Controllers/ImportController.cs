using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Services;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Stores")]
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
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [Authorize(Roles = "Admin")]
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
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [Authorize(Roles = "Admin")]
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

                        //var existingAttendanceIds = _dbContext.AttendanceIds
                        //    .Where(a => machineIds.Contains(a.MachineId))
                        //    .ToList();

                        //var existingRecords = _dbContext.AttendanceRecords
                        //    .Where(r => existingAttendanceIds.Select(a => a.Id).Contains(r.AttendanceId.Id))
                        //    .ToList();
                        var existingAttendanceMappings = _dbContext.AttendanceIds
                                    .Where(a => machineIds.Contains(a.MachineId))
                                    .ToDictionary(a => a.MachineId, a => a);

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var excelMachineIdValue = worksheet.Cells[row, 1].Value;
                                if (!int.TryParse(excelMachineIdValue?.ToString(), out int machineId))
                                {
                                    throw new Exception($"Invalid Machine ID on row {row}.");
                                }
                                if (!existingAttendanceMappings.TryGetValue(machineId, out var existingAttendanceId))
                                {
                                    throw new Exception($"Machine ID {machineId} not found.");
                                }
                                //var existingAttendanceId = existingAttendanceIds.FirstOrDefault(a => a.MachineId == machineId);
                                //if (existingAttendanceId == null)
                                //{
                                //    throw new Exception($"Machine ID {machineId} not found.");
                                //}

                                if (!DateTime.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out DateTime date))
                                {
                                    throw new Exception($"Invalid date format on row {row}.");
                                }

                                //var existingRecord = existingRecords.FirstOrDefault(r => r.AttendanceId.Id == existingAttendanceId.Id && r.Date == date);
                                //if (existingRecord != null)
                                //{
                                //    throw new Exception($"Record already exists for Machine ID {machineId} on date {date:dd-MMM-yy}.");
                                //}
                                var existingRecordExists = _dbContext.AttendanceRecords
                                             .Any(r => r.AttendanceId.Id == existingAttendanceId.Id && r.Date == date);
                                if (existingRecordExists)
                                {
                                    throw new Exception($"Record already exists for Machine ID {machineId} on date {date:dd-MMM-yy}.");
                                }

                                var checkIns = worksheet.Cells[row, 4].Value?.ToString();
                                var checkOuts = worksheet.Cells[row, 5].Value?.ToString();

                                if (string.IsNullOrEmpty(checkIns) && string.IsNullOrEmpty(checkOuts))
                                {
                                    checkIns = "";
                                    checkOuts = "";
                                }
                                else if (string.IsNullOrEmpty(checkIns) || string.IsNullOrEmpty(checkOuts))
                                {
                                    throw new Exception($"One of Check-Ins or Check-Outs is null for Machine ID {machineId} on date {date:dd-MMM-yy}.");
                                }


                                var Intimes = checkIns.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                                var OutTimes = checkOuts.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                                var checkInList = new List<string>();    
                                var checkOutList = new List<string>();
                                
                                for (int i = 0; i < Intimes.Length; i += 2)
                                {
                                    if (i + 1 < Intimes.Length)
                                    {
                                        checkInList.Add(Intimes[i] + " " + Intimes[i + 1]);
                                    }
                                }
                                for (int i = 0; i < OutTimes.Length; i += 2)
                                {
                                    if (i + 1 < OutTimes.Length)
                                    {
                                        checkOutList.Add(OutTimes[i] + " " + OutTimes[i + 1]);
                                    }
                                }

                                if (checkInList.Count != checkOutList.Count)
                                {
                                    throw new Exception($"The number of Check-Ins ({checkInList.Count}) does not match the number of Check-Outs ({checkOutList.Count}) for Machine ID {machineId} on date {date:dd-MMM-yy}.");
                                }
                                var requiredTimeCell = worksheet.Cells[row, 6].Value;
                                if (requiredTimeCell == null || !TimeSpan.TryParse(requiredTimeCell.ToString(), out TimeSpan requiredTime))
                                {
                                    throw new Exception($"Invalid Required Time format on row {row}.");
                                }
                                var lateInCell = worksheet.Cells[row, 12].Value;
                                if (lateInCell == null || !TimeSpan.TryParse(lateInCell.ToString(), out TimeSpan lateIn))
                                {
                                    throw new Exception($"Invalid Late In format on row {row}.");
                                }

                                TimeSpan totalWorkedTime = TimeSpan.Zero;

                                for (int i = 0; i < checkInList.Count; i++)
                                {
                                    var checkInStr = checkInList[i].Trim();
                                    var checkOutStr = checkOutList[i].Trim();

                                    if (checkInStr.EndsWith("PM") && !checkInStr.StartsWith("12"))
                                    {
                                        checkInStr = (Convert.ToInt32(checkInStr.Substring(0, checkInStr.IndexOf(':'))) + 12) + checkInStr.Substring(checkInStr.IndexOf(':'));
                                    }
                                    else if (checkInStr.EndsWith("AM") && checkInStr.StartsWith("12"))
                                    {
                                        checkInStr = "00" + checkInStr.Substring(checkInStr.IndexOf(':'));
                                    }

                                    if (checkOutStr.EndsWith("PM") && !checkOutStr.StartsWith("12"))
                                    {
                                        checkOutStr = (Convert.ToInt32(checkOutStr.Substring(0, checkOutStr.IndexOf(':'))) + 12) + checkOutStr.Substring(checkOutStr.IndexOf(':'));
                                    }
                                    else if (checkOutStr.EndsWith("AM") && checkOutStr.StartsWith("12"))
                                    {
                                        checkOutStr = "00" + checkOutStr.Substring(checkOutStr.IndexOf(':'));
                                    }

                                    DateTime checkInTime = DateTime.Parse(checkInStr);
                                    DateTime checkOutTime = DateTime.Parse(checkOutStr);

                                    if (checkOutTime < checkInTime)
                                    {
                                        checkOutTime = checkOutTime.AddDays(1);
                                    }

                                    TimeSpan workedTime = checkOutTime - checkInTime;
                                    totalWorkedTime += workedTime;
                                }

                                var attendanceRecord = new AttendanceRecord
                                {
                                    AttendanceId = existingAttendanceId,
                                    Date = date,
                                    CheckIns = checkIns,
                                    CheckOuts = checkOuts,
                                    RequiredTime = requiredTime,
                                    ActualTime = totalWorkedTime,
                                    TimeTable = worksheet.Cells[row, 8].Value?.ToString(),
                                    LateIn = lateIn,
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
                            catch (Exception ex)
                            {
                                return BadRequest(new { message = $"Error in row {row}: {ex.Message}" });
                            }
                        }
                    }
                }

                await _attendanceRecordService.SaveAttendanceRecordsAsync(attendanceRecords);

                return Ok(new { message = $"{attendanceRecords.Count} records processed successfully."});
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }


    }
}
