using System.ComponentModel;
using LGMS.Data.Context;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Data.Model
{
    public class Employee
    {
        public int Id { get; set; }
        public int AttendanceId { get; set; }
        public AttendanceId? AttandanceId { get; set; }
        public string Name { get; set; }
        public string EmployeeNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public Department Department { get; set; }
        public Designation Designation { get; set; }
        public DateTime JoiningDate { get; set; }
        public int BasicSalary { get; set; }
        public DateTime AgreementExpiration { get; set; }
        public EmployeeStatus Status { get; set; }
        public List<Equipment> Equipments { get; set; }

    }

}
