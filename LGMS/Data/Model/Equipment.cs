namespace LGMS.Data.Model
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public List<Employee>? Assignees { get; set; }
        public Equipment? ParentEquipment{ get; set; }
        public EquipmentStatus Status { get; set; }
        public string? Description { get; set; }
        public Vendor? Vendor { get; set; }
        public EquipmentType Type { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime? BuyingDate { get; set; }
        public DateTime? UnboxingDate { get; set; }
    }
}
