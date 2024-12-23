namespace LGMS.Dto
{
    public class DomainDetailsAddModel
    {
        public string DomainName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsExistingDomain { get; set; }
        public bool? IsOwned { get; set; }
        public string? Platform { get; set; }
        public string? PlatformAccountName { get; set; }
        public string? TransferredTo { get; set; }
        public DateTime? TransferredDate { get; set; }
        public int? ContractId { get; set; }
    }
}
