using LGMS.Data.Model;
using LGMS.Interface;

namespace LGMS.Dto
{
    public class EquipmentsSearchModel : BaseSearchModel
    {
        public List<EquipmentStatus> Statuses { get; set; }
    }
}
