﻿using LGMS.Data.Model;

namespace LGMS.Dto
{
    public class EmployeeAddModel
    {
        public string EmployeeName { get; set; }
        public int AttandanceId { get; set; }
        public DateTime BirthDate { get; set; }
        public Department Department { get; set; }
        public Designation Designation { get; set; }
        public DateTime JoiningDate { get; set; }
        public int BasicSalary { get; set; }
        public DateTime AgreementExpiration { get; set; }
        public EmployeeStatus Status { get; set; }
    }
}