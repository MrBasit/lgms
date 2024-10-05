using LGMS.Data.Model;
using LGMS.Enum;
using LGMS.Interface;

namespace LGMS.Dto
{
    public class EmployeesSearchModel
    {
        public EmployeesSearchModel()
        {
            SortDetails = new SortRequestModel();
            PaginationDetails = new PagedDataRequestModel();
            SearchDetails = new SearchRequestModel();
        }
        public ISortRequestModel SortDetails { get; set; }
        public IPagedDataRequestModel PaginationDetails { get; set; }
        public ISearchRequestModel SearchDetails { get; set; }
        public List<EmployeeStatus> Statuses { get; set; }
    }
}
