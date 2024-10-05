using LGMS.Data.Model;
using LGMS.Interface;

namespace LGMS.Dto
{
    public class AttendanceRecordSearchModel
    {
        public AttendanceRecordSearchModel()
        {
            SortDetails = new SortRequestModel();
            PaginationDetails = new PagedDataRequestModel();
            SearchDetails = new SearchRequestModel();
        }
        public ISortRequestModel SortDetails { get; set; }
        public IPagedDataRequestModel PaginationDetails { get; set; }
        public ISearchRequestModel SearchDetails { get; set; }
        public List<string> MachineNames { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Date { get; set; }
    }
}
