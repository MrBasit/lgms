using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class ContractEditModel
    {
        public int Id { get; set; }
        public int ContractAmount { get; set; }
        public DateTime ExpectedCompletion { get; set; }
        public string Details { get; set; }
        public DateTime StartDate { get; set; }
        public Client Client { get; set; }
        public ContractStatus Status { get; set; }
        public List<ContractPackageInformationDTO>? ContractPackageInformations { get; set; }
        public ContractType Type { get; set; }
        public List<PaymentDTO>? Payments { get; set; }
        public List<ExpirationDTO>? Expirations { get; set; }
    }
}
