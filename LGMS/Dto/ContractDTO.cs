using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class ContractDTO
    {
        public string Number { get; set; }
        public string? ServicesInclude { get; set; }
        public string? AdditionalCharges { get; set; }
        public int ContractAmount { get; set; }
        public DateTime ExpectedCompletion { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Details { get; set; }
        public DateTime StartDate { get; set; }
        public Client Client { get; set; }
        public ContractStatus Status { get; set; }
        public List<ContractPackageInformationDTO>? ContractPackageInformations { get; set; }
        public ContractType Type { get; set; }
        public List<PaymentInvoiceDTO>? Payments { get; set; }
        public List<ExpirationDTO>? Expirations { get; set; }
    }
}
