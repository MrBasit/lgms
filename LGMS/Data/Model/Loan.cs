namespace LGMS.Data.Model
{
    public class Loan
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public int Amount { get; set; }
        public Employee Employee { get; set; }
    }
}
