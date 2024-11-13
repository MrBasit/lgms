namespace LGMS.Data.Model
{
    public class Payment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public BankAccount BankAccount { get; set; }
        public int? ContractId { get; set; }
        public Contract? Contract { get; set; }

    }
}
