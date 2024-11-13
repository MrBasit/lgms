namespace LGMS.Data.Model
{
    public class QuotationPackageInformation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int Discount { get; set; }
        public int Total { get; set; }
        public string Description { get; set; }
        public int? QuotationId { get; set; }
        public Quotation? Quotation { get; set; }

    }
}
