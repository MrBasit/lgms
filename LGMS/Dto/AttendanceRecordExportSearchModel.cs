using LGMS.Interface;

namespace LGMS.Dto
{
    public class AttendanceRecordExportSearchModel
    {
        public AttendanceRecordExportSearchModel()
        {
            SortDetails = new SortRequestModel();
            SearchDetails = new SearchRequestModel();
        }
        public ISortRequestModel SortDetails { get; set; }
        public ISearchRequestModel SearchDetails { get; set; }
        public List<string> MachineNames { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Date { get; set; }
    }
}
