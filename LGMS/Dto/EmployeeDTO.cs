using LGMS.Data.Model;
using Microsoft.AspNetCore.Identity;

namespace LGMS.Dto
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string EmployeeNumber { get; set; }
        public string? Email { get; set; }
        public string? NIC { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public Department Department { get; set; }
        public Designation Designation { get; set; }
        public DateTime JoiningDate { get; set; }
        public int BasicSalary { get; set; }
        public DateTime AgreementExpiration { get; set; }
        public EmployeeStatus Status { get; set; }
        public List<SecurityDeposit>? SecurityDeposits { get; set; }
        public List<Loan>? Loans { get; set; }
    }
}
