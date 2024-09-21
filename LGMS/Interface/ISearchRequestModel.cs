namespace LGMS.Interface
{
    public interface ISearchRequestModel
    {
        string SearchTerm { get; set; }
    }

    public class SearchRequestModel:ISearchRequestModel
    {
        public string? SearchTerm { get; set; }
    }
}
