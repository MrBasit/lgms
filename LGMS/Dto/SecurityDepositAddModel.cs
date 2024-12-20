using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class SecurityDepositAddModel
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public int Amount { get; set; }
        public int EmployeeId { get; set; }
    }
}
