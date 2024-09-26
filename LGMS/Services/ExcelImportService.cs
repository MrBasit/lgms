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
            // Set the License Context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Use NonCommercial or Commercial as appropriate

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

                var types = _dbContext.EquipmentTypes.ToDictionary(t => t.Title.ToLower(), t => t);
                var manufacturers = _dbContext.Manufacturers.ToDictionary(m => m.Name.ToLower(), m => m);
                var statuses = _dbContext.EquipmentStatus.ToDictionary(s => s.Title.ToLower(), s => s);
                var vendors = _dbContext.Vendors.ToDictionary(v => v.Name.ToLower(), v => v);
                var allAssignees = _dbContext.Employees.ToDictionary(a => a.Name.ToLower(), a => a);

                for (int row = 2; row <= rowCount; row++)
                {
                    if (!int.TryParse(worksheet.Cells[row, 1].Text, out int id))
                    {
                        continue; // Skip rows without valid IDs
                    }

                    Equipment equipment;

                    if (existingEquipments.TryGetValue(id, out equipment))
                    {
                        equipment.Assignees?.Clear(); // Clear existing assignees
                    }
                    else
                    {
                        // Create new equipment
                        equipment = new Equipment();
                        _dbContext.Equipments.Add(equipment);
                    }

                    equipment.Type = types.GetValueOrDefault(worksheet.Cells[row, 3].Text.ToLower());
                    equipment.Manufacturer = manufacturers.GetValueOrDefault(worksheet.Cells[row, 4].Text.ToLower());
                    equipment.Status = statuses.GetValueOrDefault(worksheet.Cells[row, 6].Text.ToLower());
                    equipment.Vendor = vendors.GetValueOrDefault(worksheet.Cells[row, 8].Text.ToLower());

                    string assigneesNames = worksheet.Cells[row, 5].Text;
                    equipment.Assignees = string.IsNullOrEmpty(assigneesNames) ?
                                          new List<Employee>() :
                                          assigneesNames.Split(',')
                                                        .Select(a => a.Trim().ToLower())
                                                        .Where(assigneeName => allAssignees.ContainsKey(assigneeName))
                                                        .Select(assigneeName => allAssignees[assigneeName])
                                                        .ToList();

                    equipment.Number = worksheet.Cells[row, 2].Text;
                    equipment.Comments = worksheet.Cells[row, 7].Text;

                    DateTime.TryParse(worksheet.Cells[row, 9].Text, out DateTime warrantyExpiryDate);
                    equipment.WarrantyExpiryDate = warrantyExpiryDate;

                    DateTime.TryParse(worksheet.Cells[row, 10].Text, out DateTime buyingDate);
                    equipment.BuyingDate = buyingDate;

                    DateTime.TryParse(worksheet.Cells[row, 11].Text, out DateTime unboxingDate);
                    equipment.UnboxingDate = unboxingDate;

                    equipmentList.Add(equipment);
                }

                if (equipmentList.Count > 0)
                {
                    _dbContext.SaveChanges();
                }
            }

            return equipmentList;
        }
    }
}
