using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class GenerateQuotationModel
    {
        public QuotationDTO Quotation {  get; set; }

        public List<BankAccountDTO>? BankAccounts { get; set; }
    }
}
