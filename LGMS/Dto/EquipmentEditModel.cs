using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class EquipmentEditModel
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public EquipmentType Type { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public List<EquipmentAssigneeModel>? Assignees { get; set; }
        public string? ParentEquipmentNumber { get; set; }
        public EquipmentStatus Status { get; set; }
        public string? Description { get; set; }
        public Vendor Vendor { get; set; }
        public DateTime WarrantyExpiryDate { get; set; }
        public DateTime BuyingDate { get; set; }
        public DateTime UnboxingDate { get; set; }
    }
}
