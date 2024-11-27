using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class EditAttendanceRecord
    {
        public int Id { get; set; }
        public AttendanceRecordStatus Status { get; set; }
    }
}
