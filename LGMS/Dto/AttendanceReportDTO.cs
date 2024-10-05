namespace LGMS.Dto
{
    public class AttendanceReportDTO
    {
        public string Name { get; set; }
        public int TotalDays { get; set; }
        public int OnTimes { get; set; }
        public int LateIns { get; set; }
        public int DayOffs { get; set; }
        public int Weekends { get; set; }
        public int UnderHours { get; set; }
        public int BadRecordUnderHours { get; set; }
        public int OverHours { get; set; }
        public int BadRecordOverHours { get; set; }
        public int BadRecords { get; set; }
    }
}
