using LGMS.Data.Model;
using LGMS.Interface;

namespace LGMS.Dto
{
    public class AttendanceRecordSearchModel:BaseSearchModel
    {
        public List<string> MachineNames { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Date { get; set; }
    }
}
