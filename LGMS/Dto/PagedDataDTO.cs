namespace LGMS.Dto
{
    public class PagedDataDTO<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public List<T> Data { get; set; }

    }
}
