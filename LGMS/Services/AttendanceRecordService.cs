using LGMS.Data.Context;
using LGMS.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LGMS.Services
{
    public class AttendanceRecordService
    {
        private readonly LgmsDbContext _context;

        public AttendanceRecordService(LgmsDbContext context)
        {
            _context = context;
        }

        public AttendanceRecordStatus GetStatusFromDb(string statusName)
        {
            var status = _context.AttendanceRecordStatuses
                                 .FirstOrDefault(s => s.Title == statusName);
            if (status == null)
            {
                throw new Exception($"Status {statusName} not found.");
            }
            return status;
        }

        public AttendanceRecordStatus CalculateStatus(DateTime date, string requiredWork, string actualWork, string absentTime, string lateTime, string checkIn)
        {
            if (requiredWork == "00:00:00")
                return GetStatusFromDb("Weekend");

            if (date.DayOfWeek == DayOfWeek.Sunday && actualWork == "00:00:00")
                return GetStatusFromDb("Weekend");

            if (date.DayOfWeek == DayOfWeek.Sunday && actualWork != "00:00:00")
                return GetStatusFromDb("Extra Day");

            if (date.DayOfWeek != DayOfWeek.Sunday && actualWork == "00:00:00" && string.IsNullOrEmpty(checkIn))
                return GetStatusFromDb("Day Off");

            if (lateTime == "00:00:00")
                return GetStatusFromDb("On Time");

            return GetStatusFromDb("Late In");
        }

        public int CalculateOverHours(TimeSpan requiredTime, TimeSpan actualTime)
        {
            if (actualTime.TotalMinutes > requiredTime.TotalMinutes + 45)
            {
                return (int)((actualTime.TotalMinutes - requiredTime.TotalMinutes) / 60);
            }
            return 0;
        }

        public int CalculateUnderHours(TimeSpan requiredTime, TimeSpan actualTime, string status)
        {
            if ((status == "Weekend" || status == "Day Off") && actualTime.TotalMinutes == 0) return 0;
            if (status != "Day Off" && actualTime.TotalMinutes == 0) return 0;

            if (requiredTime > actualTime)
            {
                return (int)((requiredTime.TotalMinutes - actualTime.TotalMinutes) / 60);
            }
            return 0;
        }

        public void SaveAttendanceRecords(List<AttendanceRecord> records)
        {
            foreach (var record in records)
            {
                _context.AttendanceRecords.Add(record);
            }
            _context.SaveChanges();
        }
    }
}
