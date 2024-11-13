namespace LGMS.Data.Model
{
    public class Quotation
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public Client Client { get; set; }
        public List<BankAccount>? BankAccounts { get; set; }
        public List<QuotationPackageInformation>? QuotationPackageInformations { get; set; }
    }
}
