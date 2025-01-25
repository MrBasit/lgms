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
        private readonly Dictionary<string, AttendanceRecordStatus> _statuses;

        public AttendanceRecordService(LgmsDbContext context)
        {
            _context = context;
            _statuses = _context.AttendanceRecordStatuses.ToDictionary(s => s.Title, s => s);
        }

        public AttendanceRecordStatus CalculateStatus(DateTime date, string requiredWork, string actualWork, string absentTime, string lateTime, string checkIn)
        {
            if (!_statuses.ContainsKey("Weekend") ||
                !_statuses.ContainsKey("Extra Day") ||
                !_statuses.ContainsKey("Day Off") ||
                !_statuses.ContainsKey("On Time") ||
                !_statuses.ContainsKey("Late In"))
            {
                throw new Exception("One or more statuses are missing in the database.");
            }

            if (requiredWork == "00:00:00" && actualWork == "00:00:00")
                return _statuses["Holiday"];
            
            if (requiredWork == "00:00:00" && actualWork != "00:00:00")
                return _statuses["Extra Day"];

            if (date.DayOfWeek == DayOfWeek.Sunday && actualWork == "00:00:00")
                return _statuses["Weekend"];

            if (date.DayOfWeek == DayOfWeek.Sunday && actualWork != "00:00:00")
                return _statuses["Extra Day"];

            if (date.DayOfWeek != DayOfWeek.Sunday && actualWork == "00:00:00" && string.IsNullOrEmpty(checkIn))
                return _statuses["Day Off"];

            if (lateTime == "00:00:00")
                return _statuses["On Time"];

            return _statuses["Late In"];
        }

        public int CalculateOverHours(TimeSpan requiredTime, TimeSpan actualTime)
        {
            var requiredTotalMinutes = (int)requiredTime.TotalMinutes;
            var actualTotalMinutes = (int)actualTime.TotalMinutes;
            var totalMinutes = actualTotalMinutes - requiredTotalMinutes;

            if (totalMinutes > 45)
            {
                int overtimeHours = (int)Math.Floor((double)totalMinutes / 60);
                overtimeHours += (totalMinutes % 60 > 45 ? 1 : 0);
                return overtimeHours;
            }

            return 0;
        }

        public int CalculateUnderHours(TimeSpan requiredTime, TimeSpan actualTime, string status)
        {
            if ((status == "Weekend" || status == "Day Off") && actualTime.TotalMinutes == 0) return 0;
            if (status != "Day Off" && actualTime.TotalMinutes == 0) return 0;
            var requiredTotalMinutes = (int)requiredTime.TotalMinutes;
            var actualTotalMinutes = (int)actualTime.TotalMinutes;
            var totalMinutes = requiredTotalMinutes - actualTotalMinutes;

            if (totalMinutes > 45)
            {
                int underTimeHours = (int)Math.Floor((double)totalMinutes / 60);
                underTimeHours += (totalMinutes % 60 > 45 ? 1 : 0);
                return underTimeHours;
            }
            return 0;
        }

        public async Task SaveAttendanceRecordsAsync(List<AttendanceRecord> records)
        {
            foreach (var record in records)
            {
                await _context.AttendanceRecords.AddAsync(record);
            }
            await _context.SaveChangesAsync();
        }
    }
}
