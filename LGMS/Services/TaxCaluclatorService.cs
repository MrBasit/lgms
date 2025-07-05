using LGMS.Model;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace LGMS.Services
{
    public class TaxCalculatorService
    {
        private readonly List<TaxSlab> _slabs;

        public TaxCalculatorService(IOptions<IncomeTaxSettings> options)
        {
            _slabs = options.Value.Slabs.OrderBy(s => s.Min).ToList();
        }

        public decimal CalculateMonthlyTax(decimal monthlyIncome)
        {
            decimal annualIncome = monthlyIncome * 12;
            decimal annualTax = 0;

            foreach (var slab in _slabs)
            {
                if (annualIncome <= slab.Min)
                    break;

                decimal upperLimit = slab.Max ?? annualIncome;
                decimal taxableAmount = Math.Min(annualIncome, upperLimit) - slab.Min;

                if (taxableAmount > 0)
                    annualTax += taxableAmount * slab.Rate;
            }

            return Math.Round(annualTax / 12, 2); // Monthly tax rounded to 2 decimals
        }

        public decimal CalculateTaxableAmount(int Salary, bool OnTimeAllowance, bool AttendanceAllowance, bool PerformanceAllowance, int Overtime, int Comission, int Deductions, bool DeductionApplied)
        {
            var totalTaxableAmount = 0.0;
            totalTaxableAmount += (Salary + (OnTimeAllowance ? Salary * 0.1 : 0) + (AttendanceAllowance ? Salary * 0.1 : 0) + (PerformanceAllowance ? Salary * 0.1 : 0));
            totalTaxableAmount += (Overtime + Comission);
            totalTaxableAmount -= DeductionApplied ? Deductions : 0;

            return (decimal)totalTaxableAmount;
            

        }
    }

}
