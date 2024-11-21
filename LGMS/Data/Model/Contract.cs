namespace LGMS.Data.Model
{
    public class Contract
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int ContractAmount { get; set; }
        public DateTime ExpectedCompletion { get; set; }
        public string Details { get; set; }
        public DateTime StartDate { get; set; }
        public Client Client { get; set; }
        public ContractStatus Status { get; set; }
        public List<ContractPackageInformation>? ContractPackageInformations { get; set; }
        public ContractType Type { get; set; }
        public List<Payment>? Payments { get; set; }
        public List<Expiration>? Expirations { get; set; }
        public List<DomainDetails>?  DomainDetails{ get; set; }
    }
}
