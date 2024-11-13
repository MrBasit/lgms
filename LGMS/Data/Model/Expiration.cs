namespace LGMS.Data.Model
{
    public class Expiration
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public int? ContractId { get; set; }
        public Contract? Contract { get; set; }
    }
}
