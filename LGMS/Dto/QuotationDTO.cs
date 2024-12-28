using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class QuotationDTO
    {
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public Client Client { get; set; }
        public List<QuotationPackageInformationDTO>? QuotationPackageInformations { get; set; }
    }
}
