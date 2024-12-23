namespace LGMS.Dto
{
    public class BankAccountAddModel
    {
        public string AccountTitle { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string? IBAN { get; set; }
    }
}
