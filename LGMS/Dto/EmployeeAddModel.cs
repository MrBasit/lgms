using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class EmployeeAddModel
    {
        public string EmployeeName { get; set; }
        public string? AttendanceId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NIC { get; set; }
        public DateTime BirthDate { get; set; }
        public Department Department { get; set; }
        public Designation Designation { get; set; }
        public DateTime JoiningDate { get; set; }
        public int BasicSalary { get; set; }
        public DateTime AgreementExpiration { get; set; }
        public EmployeeStatus Status { get; set; }
    }
}
