namespace LGMS.Data.Model
{
    public class SalarySlip
    {
        public int Id { get; set; }
        public Employee Employee { get; set; }
        public DateTime? GenratedDate { get; set; }
        public DateTime? PayPeriod { get; set; }
        public int Salary { get; set; }
        public int Deductions { get; set; }
        public bool OnTimeAllowance { get; set; }
        public bool AttendanceAllowance { get; set; }
        public bool? PerformanceAllowance { get; set; }
        public bool Paid { get; set; } = false;
        public bool DeductionApplied { get; set; } = false;
        public int Overtime { get; set; }
        public int? SecurityDeposit { get; set; }
        public int? IncomeTax { get; set; }
        public int? Loan { get; set; }
        public int? Comission { get; set; }
        public int Total { get; set; }
    }
}
