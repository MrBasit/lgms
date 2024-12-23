using LGMS.Data.Model;
using LGMS.Interface;

namespace LGMS.Dto
{
    public class EquipmentExportSearchModel
    {
        public EquipmentExportSearchModel()
        {
            SortDetails = new SortRequestModel();
            SearchDetails = new SearchRequestModel();
        }
        public ISortRequestModel SortDetails { get; set; }
        public ISearchRequestModel SearchDetails { get; set; }
        public List<EquipmentStatus> Statuses { get; set; }
    }
}
