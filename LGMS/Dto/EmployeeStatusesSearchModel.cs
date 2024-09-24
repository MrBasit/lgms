using LGMS.Interface;

namespace LGMS.Dto
{
    public class EmployeeStatusesSearchModel
    {
        public EmployeeStatusesSearchModel()
        {
            SortDetails = new SortRequestModel();
            PaginationDetails = new PagedDataRequestModel();
            SearchDetails = new SearchRequestModel();
        }
        public ISortRequestModel SortDetails { get; set; }
        public IPagedDataRequestModel PaginationDetails { get; set; }
        public ISearchRequestModel SearchDetails { get; set; }
    }
}
