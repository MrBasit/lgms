namespace LGMS.Dto
{
    public class SalarySlipDTO
    {
        public string Name { get; set; }
        public int Salary { get; set; }
        public int Deductions { get; set; }
        public bool OnTimeAllowance { get; set; }
        public bool AttendanceAllowance { get; set; }
        public int Overtime { get; set; }
        public int Total { get; set; }
    }

}
