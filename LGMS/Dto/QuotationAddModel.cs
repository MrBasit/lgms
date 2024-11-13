using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class QuotationAddModel
    {
        public DateTime Date { get; set; }
        public int ClientId { get; set; }
        public List<BankAccount>? BankAccounts { get; set; }
        public List<QuotationPackageInformationDTO>? PackageInformation { get; set; }
    }
}
