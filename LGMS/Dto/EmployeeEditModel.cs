using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class EmployeeEditModel
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string? AttendanceId {get; set; }
        public DateTime BirthDate { get; set; }
        public Department Department { get; set; }
        public Designation Designation { get; set; }
        public DateTime JoiningDate { get; set; }
        public int BasicSalary { get; set; }
        public DateTime AgreementExpiration { get; set; }
        public EmployeeStatus Status { get; set; }
    }
}
