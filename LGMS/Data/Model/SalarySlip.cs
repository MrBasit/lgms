namespace LGMS.Data.Model
{
    public class SalarySlip
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Designation { get; set; }
        public DateTime UpdatedDate{ get; set; }
        public int Salary { get; set; }
        public int Deductions { get; set; }
        public bool OnTimeAllowance { get; set; }
        public bool AttendanceAllowance { get; set; }
        public bool? PerformanceAllowance { get; set; }
        public int Overtime { get; set; }
        public int? Comission { get; set; }
        public int Total { get; set; }
    }
}
