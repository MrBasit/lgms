namespace LGMS.Data.Model
{
    public class ContractPackageInformation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int Discount { get; set; }
        public int Total { get; set; }
        public string Description { get; set; }
        public int? ContractId { get; set; }
        public Contract? Contract { get; set; }
    }
}
