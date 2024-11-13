using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class PaymentDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public BankAccountDTO BankAccount { get; set; }
    }
}
