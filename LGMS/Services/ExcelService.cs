using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Collections.Generic;
using System.IO;
using LGMS.Data.Model;

namespace LGMS.Services
{
    public class ExcelService
    {
        public byte[] GenerateEquipmentExcelFile(List<Equipment> data)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Equipment Data");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Equipment Number";
                worksheet.Cells[1, 3].Value = "Equipment Type";
                worksheet.Cells[1, 4].Value = "Manufacturer";
                worksheet.Cells[1, 5].Value = "Assignees";
                worksheet.Cells[1, 6].Value = "Status";
                worksheet.Cells[1, 7].Value = "Comments";
                worksheet.Cells[1, 8].Value = "Vendor";
                worksheet.Cells[1, 9].Value = "Warranty Duration";
                worksheet.Cells[1, 10].Value = "Buying Date";
                worksheet.Cells[1, 11].Value = "Unboxing Date";

                using (var headerRange = worksheet.Cells[1, 1, 1, 11])
                {
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = data[i].Id;
                    worksheet.Cells[i + 2, 2].Value = data[i].Number;
                    worksheet.Cells[i + 2, 3].Value = data[i].Type.Title;
                    worksheet.Cells[i + 2, 4].Value = data[i].Manufacturer.Name;
                    worksheet.Cells[i + 2, 5].Value = string.Join(", ", data[i].Assignees.Select(a => a.Name));
                    worksheet.Cells[i + 2, 6].Value = data[i].Status.Title;
                    worksheet.Cells[i + 2, 7].Value = data[i].Comments;
                    worksheet.Cells[i + 2, 8].Value = data[i].Vendor.Name;
                    worksheet.Cells[i + 2, 9].Value = data[i].WarrantyExpiryDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 10].Value = data[i].BuyingDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 11].Value = data[i].UnboxingDate.ToString("yyyy-MM-dd");
                }

                using (var dataRange = worksheet.Cells[1, 1, data.Count + 1, 11])
                {
                    dataRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }

        }

        public byte[] GenerateAttendanceRecordExcelFile(List<AttendanceRecord> data)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Attendance Data");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Date";
                worksheet.Cells[1, 4].Value = "CheckIns";
                worksheet.Cells[1, 5].Value = "CheckOuts";
                worksheet.Cells[1, 6].Value = "Status";
                worksheet.Cells[1, 7].Value = "Required Time";
                worksheet.Cells[1, 8].Value = "Actual Time";
                worksheet.Cells[1, 9].Value = "Under Hours";
                worksheet.Cells[1, 10].Value = "Over Hours";
                worksheet.Cells[1, 11].Value = "Is Record OK";

                using (var headerRange = worksheet.Cells[1, 1, 1, 11])
                {
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = data[i].Id;
                    worksheet.Cells[i + 2, 2].Value = data[i].AttendanceId.MachineName;
                    worksheet.Cells[i + 2, 3].Value = data[i].Date.ToString("dd-MMM-yy");
                    worksheet.Cells[i + 2, 4].Value = data[i].CheckIns;
                    worksheet.Cells[i + 2, 5].Value = data[i].CheckOuts;
                    worksheet.Cells[i + 2, 6].Value = data[i].Status.Title;
                    worksheet.Cells[i + 2, 7].Value = data[i].RequiredTime.ToString(@"hh\:mm");
                    worksheet.Cells[i + 2, 8].Value = data[i].ActualTime.ToString(@"hh\:mm");
                    worksheet.Cells[i + 2, 9].Value = data[i].UnderHours;
                    worksheet.Cells[i + 2, 10].Value = data[i].OverHours;
                    worksheet.Cells[i + 2, 11].Value = data[i].IsRecordOk ? "Yes" : "No";
                }

                using (var dataRange = worksheet.Cells[1, 1, data.Count + 1, 11])
                {
                    dataRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }

        }
    }
}
