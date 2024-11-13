using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LGMS.Data.Context;
using LGMS.Data.Model;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.PortableExecutable;

namespace LGMS.Services
{
    public class ExcelImportService
    {
        private readonly LgmsDbContext _dbContext;
        private readonly AttendanceRecordService _attendanceRecordService;


        public ExcelImportService(LgmsDbContext dbContext, AttendanceRecordService attendanceRecordService)
        {
            _dbContext = dbContext;
            _attendanceRecordService = attendanceRecordService;
        }

        public List<Equipment> ParseEquipmentExcelFile(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var equipmentList = new List<Equipment>();
            var excelIds = new HashSet<int>();

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    if (int.TryParse(worksheet.Cells[row, 1].Text, out int id))
                    {
                        excelIds.Add(id);
                    }
                }

                var existingEquipments = _dbContext.Equipments
                    .Include(e => e.Assignees)
                    .Where(e => excelIds.Contains(e.Id))
                    .ToDictionary(e => e.Id);

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        if (!int.TryParse(worksheet.Cells[row, 1].Text, out int id))
                        {
                            throw new Exception($"Invalid Equipment ID.");
                        }

                        Equipment equipment;

                        var equipmentNumber = worksheet.Cells[row, 2].Text;

                        if (id == 0)
                        {
                            equipment = new Equipment();
                            _dbContext.Equipments.Add(equipment);
                        }
                        else if (existingEquipments.TryGetValue(id, out equipment))
                        {
                            equipment.Assignees?.Clear();
                        }
                        else
                        {
                            throw new Exception($"Equipment with ID {id} not found in the database.");
                        }

                        if (_dbContext.Equipments.Any(e => e.Number.ToUpper() == equipmentNumber.ToUpper() && e.Id != id))
                        {
                            throw new Exception($"Equipment number {equipmentNumber} already exists.");
                        }
                        equipment.Number = equipmentNumber;



                        var parentEquipmentNo = worksheet.Cells[row, 3].Text;

                        if (string.IsNullOrEmpty(parentEquipmentNo))
                        {
                            equipment.ParentEquipment = null;
                        }
                        else
                        {
                            var parentEquipment = _dbContext.Equipments
                                .Include(e => e.ParentEquipment)
                                .SingleOrDefault(e => e.Number.ToUpper() == parentEquipmentNo.ToUpper().Trim());

                            if (parentEquipment == null)
                            {
                                throw new Exception("No Equipment found with this number");
                            }
                            if (equipment.Number.ToUpper() == parentEquipmentNo.ToUpper().Trim())
                            {
                                throw new Exception("Equipment cannot be its own parent.");
                            }
                            if (parentEquipment != null && parentEquipment.ParentEquipment == equipment)
                            {
                                throw new Exception("Cannot assign parent equipment as it is a child of the equipment being edited.");
                            }

                            equipment.ParentEquipment = parentEquipment;
                        }

                        string typeTitle = worksheet.Cells[row, 4].Text.ToLower();
                        var type = _dbContext.EquipmentTypes.FirstOrDefault(t => t.Title.ToLower() == typeTitle);
                        if (type == null)
                        {
                            throw new Exception($"Equipment Type not found.");
                        }
                        equipment.Type = type;

                        string assigneesNames = worksheet.Cells[row, 5].Text;
                        if (!string.IsNullOrEmpty(assigneesNames))
                        {
                            var assigneesList = assigneesNames.Split(',')
                            .Select(a => a.Trim().ToLower())
                            .Select(assigneeName =>
                            {
                                var assignee = _dbContext.Employees.FirstOrDefault(e => e.Name.ToLower() == assigneeName);
                                if (assignee == null)
                                {
                                    throw new Exception($"Assignee '{assigneeName}' not found in the database.");
                                }
                                return assignee;
                            })
                            .ToList();

                            equipment.Assignees = assigneesList;
                        }


                        string statusTitle = worksheet.Cells[row, 6].Text.ToLower();
                        var status = _dbContext.EquipmentStatus.FirstOrDefault(s => s.Title.ToLower() == statusTitle);
                        if (status == null)
                        {
                            throw new Exception($"Status not found.");
                        }
                        equipment.Status = status;


                        if (!string.IsNullOrEmpty(worksheet.Cells[row, 7].Text))
                        {
                            if (DateTime.TryParse(worksheet.Cells[row, 7].Text, out DateTime buyingDate))
                            {
                                equipment.BuyingDate = buyingDate;
                            }
                            else
                            {
                                throw new Exception($"Invalid Buying Date.");
                            }
                        }
                        else
                        {
                            equipment.BuyingDate = null;
                        }

                        if (!string.IsNullOrEmpty(worksheet.Cells[row, 8].Text))
                        {
                            if (DateTime.TryParse(worksheet.Cells[row, 8].Text, out DateTime unboxingDate))
                            {
                                equipment.UnboxingDate = unboxingDate;
                            }
                            else
                            {
                                throw new Exception($"Invalid Unboxing Date.");
                            }
                        }
                        else
                        {
                            equipment.UnboxingDate = null;
                        }

                        if (!string.IsNullOrEmpty(worksheet.Cells[row, 9].Text))
                        {
                            if (DateTime.TryParse(worksheet.Cells[row, 9].Text, out DateTime warrantyExpiryDate))
                            {
                                equipment.WarrantyExpiryDate = warrantyExpiryDate;
                            }
                            else
                            {
                                throw new Exception($"Invalid Warranty Expiry Date.");
                            }
                        }
                        else
                        {
                            equipment.WarrantyExpiryDate = null;
                        }


                        string manufacturerName = worksheet.Cells[row, 10].Text.ToLower();
                        if (!string.IsNullOrEmpty(manufacturerName))
                        {
                            var manufacturer = _dbContext.Manufacturers.FirstOrDefault(m => m.Name.ToLower() == manufacturerName);
                            if (manufacturer == null)
                            {
                                throw new Exception($"Manufacturer not found.");
                            }
                            equipment.Manufacturer = manufacturer;
                        }
                        else
                        {
                            equipment.Manufacturer = null;
                        }
                        


                        string vendorName = worksheet.Cells[row, 11].Text.ToLower();
                        if (!string.IsNullOrEmpty(vendorName))
                        {
                            var vendor = _dbContext.Vendors.FirstOrDefault(v => v.Name.ToLower() == vendorName);
                            if (vendor == null)
                            {
                                throw new Exception($"Vendor not found.");
                            }
                             equipment.Vendor = vendor;
                        }
                        else
                        {
                            equipment.Vendor = null;
                        }


                        equipment.Description = worksheet.Cells[row, 12].Text;

                        equipmentList.Add(equipment);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error in row {row}: {ex.Message}");
                    }
                }
                if (equipmentList.Count > 0)
                {
                    _dbContext.SaveChanges();
                }
            }

            return equipmentList;
        }



        public List<AttendanceRecord> ParseAttendanceRecordExcelFile(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var attendanceRecordList = new List<AttendanceRecord>();
            var excelIds = new HashSet<int>();

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    if (int.TryParse(worksheet.Cells[row, 1].Text, out int id))
                    {
                        excelIds.Add(id);
                    }
                }

                var existingAttendanceRecords = _dbContext.AttendanceRecords
                    .Include(e => e.AttendanceId)
                    .Include(a => a.Status)
                    .Where(e => excelIds.Contains(e.Id))
                    .ToDictionary(e => e.Id);

                for (int row = 2; row <= rowCount; row++)
                {
                    if (!int.TryParse(worksheet.Cells[row, 1].Text, out int id))
                    {
                        throw new Exception($"Error in row {row}:Value '{worksheet.Cells[row, 1].Text}' is not a valid ID.");
                    }

                    if (!existingAttendanceRecords.TryGetValue(id, out AttendanceRecord attendanceRecord))
                    {
                        throw new Exception($"Error in row {row}:No attendance record found for id '{worksheet.Cells[row, 1].Text}'.");
                    }

                    try
                    {
                        var checkIns = worksheet.Cells[row, 5].Value?.ToString();
                        var checkOuts = worksheet.Cells[row, 6].Value?.ToString();

                        if (string.IsNullOrEmpty(checkIns) && string.IsNullOrEmpty(checkOuts))
                        {
                            checkIns = "";
                            checkOuts = "";
                        }
                        else if (string.IsNullOrEmpty(checkIns) || string.IsNullOrEmpty(checkOuts))
                        {
                            throw new Exception($"One of Check-Ins or Check-Outs is null for Machine ID {attendanceRecord.AttendanceId.MachineId} on date {attendanceRecord.Date:dd-MMM-yy}.");
                        }


                        var Intimes = checkIns.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var OutTimes = checkOuts.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

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
                            throw new Exception($"The number of Check-Ins ({checkInList.Count}) does not match the number of Check-Outs ({checkOutList.Count}) for Machine ID {attendanceRecord.AttendanceId.MachineId} on date {attendanceRecord.Date:dd-MMM-yy}.");
                        }
                        attendanceRecord.CheckIns = checkIns;
                        attendanceRecord.CheckOuts = checkOuts; 


                        TimeSpan totalWorkedTime = TimeSpan.Zero;
                        if (worksheet.Cells[row, 7].Value == null)
                        {
                            throw new Exception($"Required Time is missing for Machine ID {attendanceRecord.AttendanceId.MachineId} on date {attendanceRecord.Date:dd-MMM-yy}.");
                        }
                        if (worksheet.Cells[row, 8].Value == null)
                        {
                            throw new Exception($"Late In time is missing for Machine ID {attendanceRecord.AttendanceId.MachineId} on date {attendanceRecord.Date:dd-MMM-yy}.");
                        }

                        TimeSpan requiredTime;
                        try
                        {
                            requiredTime = TimeSpan.Parse(worksheet.Cells[row, 7].Value.ToString());
                        }
                        catch
                        {
                            throw new Exception($"Required Time is invalid for Machine ID {attendanceRecord.AttendanceId.MachineId} on date {attendanceRecord.Date:dd-MMM-yy}.");
                        }

                        TimeSpan lateIn;
                        try
                        {
                            lateIn = TimeSpan.Parse(worksheet.Cells[row, 8].Value.ToString());
                        }
                        catch
                        {
                            throw new Exception($"Late In Time is invalid for Machine ID {attendanceRecord.AttendanceId.MachineId} on date {attendanceRecord.Date:dd-MMM-yy}.");
                        }

                        attendanceRecord.RequiredTime = requiredTime;
                        attendanceRecord.LateIn = lateIn;

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
                        attendanceRecord.ActualTime = totalWorkedTime;

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


                        attendanceRecordList.Add(attendanceRecord);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error in row {row}: {ex.Message}");
                    }
                }

                if (attendanceRecordList.Count > 0)
                {
                    _dbContext.SaveChanges();
                }
            }

            return attendanceRecordList;
        }


    }
}
