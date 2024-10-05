using LGMS.Data.Model;
using LGMS.Dto;

namespace LGMS.Services
{
    public class AttendanceReportService
    {
        public AttendanceReportDTO GenerateReport(string employeeName, List<AttendanceRecord> attendanceRecords)
        {
            var report = new AttendanceReportDTO
            {
                Name = employeeName,
                OnTimes = attendanceRecords.Count(a => a.Status.Title == "On Time"),
                LateIns = attendanceRecords.Count(a => a.Status.Title == "Late In"),
                DayOffs = attendanceRecords.Count(a => a.Status.Title == "Day Off"),
                Weekends = attendanceRecords.Count(a => a.Status.Title == "Weekend"),
                TotalDays = 0,
                UnderHours = attendanceRecords.Where(a => a.IsRecordOk).Sum(a => a.UnderHours),
                BadRecordUnderHours = attendanceRecords.Where(a => !a.IsRecordOk).Sum(a => a.UnderHours),
                OverHours = attendanceRecords.Where(a => a.IsRecordOk).Sum(a => a.OverHours),
                BadRecordOverHours = attendanceRecords.Where(a => !a.IsRecordOk).Sum(a => a.OverHours),
                BadRecords = attendanceRecords.Count(a => !a.IsRecordOk)
            };

            report.TotalDays = report.OnTimes + report.LateIns + report.DayOffs + report.Weekends;

            return report;
        }
    }
}
