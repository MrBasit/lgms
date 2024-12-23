using LGMS.Interface;

namespace LGMS.Dto
{
    public class AttendanceReportSearchModel : BaseSearchModel
    {
        public List<string> MachineNames { get; set; }
        public int Year { get; set; }
        public List<int> Months { get; set; }
    }
}
