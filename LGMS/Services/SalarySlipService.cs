using LGMS.Data.Model;
using LGMS.Dto;

namespace LGMS.Services
{
    public class SalarySlipService
    {
        public SalarySlipDTO GenerateSalarySlip(AttendanceReportDTO report, int year, int month, Employee employee)
        {
            var salarySlipDate = new DateTime(year, month, 1);
            var salarySlip = new SalarySlipDTO
            {
                Employee = employee,
                GenratedDate = DateTime.Now,
                PayPeriod = salarySlipDate,
                Salary = employee.BasicSalary,
                Deductions = CalculateDeductions(report, employee.BasicSalary),
                OnTimeAllowance = CalculateOnTimeAllowance(report),
                AttendanceAllowance = CalculateAttendanceAllowance(report),
                PerformanceAllowance = false,
                DeductionApplied = false,
                Overtime = CalculateOvertime(report, employee.BasicSalary),
                SecurityDeposit = 0,
                IncomeTax = 0,
                Loan = 0,
                Comission = 0
            };

            salarySlip.Total = CalculateTotal(salarySlip);
            return salarySlip;
        }

        private int CalculateDeductions(AttendanceReportDTO report, int salary)
        {
            var offs = report.DayOffs <= 2 ? 0 : report.DayOffs - 2;
            int deductions = (salary / 26) * offs;

            if (report.UnderHours - report.OverHours > 5)
            {
                var underHoursDeductions = (report.UnderHours - report.OverHours ) * (salary / (26 * 9));
                deductions += underHoursDeductions;
            }
            return deductions;
        }

        private bool CalculateOnTimeAllowance(AttendanceReportDTO report)
        {
            if (report.DayOffs > 6)
            {
                return false;
            }
            return report.LateIns <= 3;
        }

        private bool CalculateAttendanceAllowance(AttendanceReportDTO report)
        {
            return report.DayOffs == 0;
        }

        private int CalculateOvertime(AttendanceReportDTO report, int salary)
        {
            var overhours = report.OverHours - report.UnderHours > 5
                ? report.OverHours - report.UnderHours
                : 0;
            return ((salary / (13 * 9)) * overhours);
        }

        private int CalculateTotal(SalarySlipDTO slip)
        {
            int total =  slip.Salary + slip.Overtime;

            if(slip.DeductionApplied == true)
            {
                total = total - slip.Deductions;
            }

            if (slip.OnTimeAllowance)
            {
                total += (int)(slip.Salary * 0.10);
            }

            if (slip.AttendanceAllowance)
            {
                total += (int)(slip.Salary * 0.10);
            }
            return total;
        }
    }

}
