using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LGMS.Data.Context;
using LGMS.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Services
{
    public class ExcelImportService
    {
        private readonly LgmsDbContext _dbContext;

        public ExcelImportService(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
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

                        if (_dbContext.Equipments.Any(e => e.Number == equipmentNumber && e.Id != id))
                        {
                            throw new Exception($"Equipment number {equipmentNumber} already exists.");
                        }

                        string typeTitle = worksheet.Cells[row, 3].Text.ToLower();
                        var type = _dbContext.EquipmentTypes.FirstOrDefault(t => t.Title.ToLower() == typeTitle);
                        if (type == null)
                        {
                            type = new EquipmentType { Title = worksheet.Cells[row, 3].Text };
                            _dbContext.EquipmentTypes.Add(type);
                        }
                        equipment.Type = type;

                        string manufacturerName = worksheet.Cells[row, 4].Text.ToLower();
                        var manufacturer = _dbContext.Manufacturers.FirstOrDefault(m => m.Name.ToLower() == manufacturerName);
                        if (manufacturer == null)
                        {
                            manufacturer = new Manufacturer { Name = worksheet.Cells[row, 4].Text };
                            _dbContext.Manufacturers.Add(manufacturer);
                        }
                        equipment.Manufacturer = manufacturer;

                        string statusTitle = worksheet.Cells[row, 6].Text.ToLower();
                        var status = _dbContext.EquipmentStatus.FirstOrDefault(s => s.Title.ToLower() == statusTitle);
                        if (status == null)
                        {
                            status = new EquipmentStatus { Title = worksheet.Cells[row, 6].Text };
                            _dbContext.EquipmentStatus.Add(status);
                        }
                        equipment.Status = status;

                        string vendorName = worksheet.Cells[row, 8].Text.ToLower();
                        var vendor = _dbContext.Vendors.FirstOrDefault(v => v.Name.ToLower() == vendorName);
                        if (vendor == null)
                        {
                            vendor = new Vendor { Name = worksheet.Cells[row, 8].Text };
                            _dbContext.Vendors.Add(vendor);
                        }
                        equipment.Vendor = vendor;

                        string assigneesNames = worksheet.Cells[row, 5].Text;
                        if (!string.IsNullOrEmpty(assigneesNames))
                        {
                            var assigneesList = assigneesNames.Split(',')
                                .Select(a => a.Trim().ToLower())
                                .Select(assigneeName => _dbContext.Employees
                                    .FirstOrDefault(e => e.Name.ToLower() == assigneeName))
                                .Where(assignee => assignee != null)
                                .ToList();

                            equipment.Assignees = assigneesList;
                        }

                        equipment.Number = equipmentNumber;
                        equipment.Comments = worksheet.Cells[row, 7].Text;

                        DateTime.TryParse(worksheet.Cells[row, 9].Text, out DateTime warrantyExpiryDate);
                        equipment.WarrantyExpiryDate = warrantyExpiryDate;

                        DateTime.TryParse(worksheet.Cells[row, 10].Text, out DateTime buyingDate);
                        equipment.BuyingDate = buyingDate;

                        DateTime.TryParse(worksheet.Cells[row, 11].Text, out DateTime unboxingDate);
                        equipment.UnboxingDate = unboxingDate;

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
                        continue;
                    }

                    if (!existingAttendanceRecords.TryGetValue(id, out AttendanceRecord attendanceRecord))
                    {
                        continue; 
                    }

                    string machineName = worksheet.Cells[row, 2].Text;
                    if (!string.IsNullOrEmpty(machineName))
                    {
                        var name = _dbContext.AttendanceIds.SingleOrDefault(a => a.MachineName.Trim().ToLower() == machineName.Trim().ToLower());
                        if (name != null)
                        {
                            attendanceRecord.AttendanceId = name;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (DateTime.TryParse(worksheet.Cells[row, 3].Text, out DateTime date))
                    {
                        attendanceRecord.Date = date;
                    }

                    attendanceRecord.CheckIns = worksheet.Cells[row, 4].Text;
                    attendanceRecord.CheckOuts = worksheet.Cells[row, 5].Text;

                    string statusTitle = worksheet.Cells[row, 6].Text.ToLower();
                    var status = _dbContext.AttendanceRecordStatuses.FirstOrDefault(s => s.Title.ToLower() == statusTitle);
                    if (status != null)
                    {
                        attendanceRecord.Status = status;
                    }
                    else
                    {
                        continue; 
                    }

                    string requiredTimeStr = worksheet.Cells[row, 7].Value?.ToString().Trim();
                    string actualTimeStr = worksheet.Cells[row, 8].Value?.ToString().Trim();


                    if (TimeSpan.TryParse(requiredTimeStr, out TimeSpan requiredTime))
                    {
                        attendanceRecord.RequiredTime = requiredTime;
                    }
                    else
                    {
                        attendanceRecord.RequiredTime = TimeSpan.Zero; 
                    }

                    if (TimeSpan.TryParse(actualTimeStr, out TimeSpan actualTime))
                    {
                        attendanceRecord.ActualTime = actualTime;
                    }
                    else
                    {
                        attendanceRecord.ActualTime = TimeSpan.Zero; 
                    }

                    string underHoursStr = worksheet.Cells[row, 9].Value?.ToString();
                    if (int.TryParse(underHoursStr, out int underHours))
                    {
                        attendanceRecord.UnderHours = underHours;
                    }
                    else
                    {
                        attendanceRecord.UnderHours = 0;  
                    }

                    string overHoursStr = worksheet.Cells[row, 10].Value?.ToString();
                    if (int.TryParse(overHoursStr, out int overHours))
                    {
                        attendanceRecord.OverHours = overHours;
                    }
                    else
                    {
                        attendanceRecord.OverHours = 0; 
                    }

                    string isRecordOkStr = worksheet.Cells[row, 11].Value?.ToString().Trim();
                    attendanceRecord.IsRecordOk = isRecordOkStr.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                    attendanceRecord.TimeTable = worksheet.Cells[row, 12].Value?.ToString().Trim();

                    string LateInStr = worksheet.Cells[row, 13].Value?.ToString().Trim();

                    if (TimeSpan.TryParse(LateInStr, out TimeSpan LateIn))
                    {
                        attendanceRecord.LateIn = LateIn;
                    }
                    else
                    {
                        attendanceRecord.LateIn = TimeSpan.Zero;
                    }

                    

                    attendanceRecordList.Add(attendanceRecord);
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
