using LGMS.Interface;

namespace LGMS.Dto
{
    public class SalarySlipSearchModel
    {
        public SalarySlipSearchModel()
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
    }
}
