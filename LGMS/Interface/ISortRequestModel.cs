using LGMS.Enum;

namespace LGMS.Interface
{
    public interface ISortRequestModel
    {
        string SortColumn{ get; set; }

        SortDirections SortDirection{ get; set; }
    }
    public class SortRequestModel:ISortRequestModel
    {
        public string SortColumn { get; set; }

        public SortDirections SortDirection { get; set; }
    }

}
