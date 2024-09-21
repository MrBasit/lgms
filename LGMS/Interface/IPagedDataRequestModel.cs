namespace LGMS.Interface
{
    public interface IPagedDataRequestModel
    {
        int PageNumber { get; set; }
        int PageSize { get; set; }
    }

    public class PagedDataRequestModel:IPagedDataRequestModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
