using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class QuotationEditModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int ClientId { get; set; }
        public List<BankAccount>? BankAccounts { get; set; }
        public List<QuotationPackageInformationDTO>? PackageInformation { get; set; }

    }
}
