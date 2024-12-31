using LGMS.Dto;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System.Globalization;
using QuestPDF.Fluent;
using LGMS.Data.Model;


namespace LGMS.Services
{
    public class SalarySlipPDFService
    {
        public IDocument CreateSalarySlipPdf(SalarySlipDTO slip, string header, string footer)
        {
            decimal basicSalary = slip.Employee.BasicSalary;
            decimal onTimeAllowance = slip.OnTimeAllowance ? CalculateAllowances(true, basicSalary) : 0;
            decimal attendanceAllowance = slip.AttendanceAllowance ? CalculateAllowances(slip.AttendanceAllowance, basicSalary) : 0;
            decimal performanceAllowance = slip.PerformanceAllowance.HasValue && slip.PerformanceAllowance.Value ? CalculateAllowances(true, basicSalary) : 0;
            decimal grossTotal = basicSalary + onTimeAllowance + attendanceAllowance + performanceAllowance + slip.Overtime + (slip.Comission ?? 0);
            decimal totalDeductions = slip.DeductionApplied? slip.Deductions: 0;
            if (slip.Loan.HasValue)
            {
                totalDeductions += slip.Loan.Value;
            }

            if (slip.SecurityDeposit.HasValue)
            {
                totalDeductions += slip.SecurityDeposit.Value;
            }

            if (slip.IncomeTax.HasValue)
            {
                totalDeductions += slip.IncomeTax.Value;
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Height(110).AlignCenter().Image(header, ImageScaling.FitArea);

                    page.Content().PaddingRight(70).PaddingLeft(70).Column(col =>
                    {

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(110);
                                columns.RelativeColumn();
                                columns.ConstantColumn(110);
                                columns.RelativeColumn();
                            });

                            table.Cell().Padding(3).Text("Genrated Date:").Bold();
                            table.Cell().Padding(3).Text(DateTime.Now.ToString("yyyy-MM-dd"));
                            table.Cell().Padding(3).Text("Employee Name:").Bold();
                            table.Cell().Padding(3).Text(slip.Employee.Name).FontColor("#010943");

                            table.Cell().Padding(3).Text("Pay Period:").Bold();
                            table.Cell().Padding(3).Text(slip.PayPeriod.Value.ToString("MMM yyyy"));
                            table.Cell().Padding(3).Text("Designation:").Bold();
                            table.Cell().Padding(3).Text(slip.Employee.Designation.Title);

                            table.Cell().Padding(3).Text("Currency:").Bold();
                            table.Cell().Padding(3).Text("PKR");
                            table.Cell().Padding(3).Text("Department:").Bold();
                            table.Cell().Padding(3).Text(slip.Employee.Department.Name);
                        });


                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Padding(0).Table(earningsTable =>
                            {
                                earningsTable.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                earningsTable.Cell().Border(1).BorderRight(0).Background("#F27E63").Padding(3).Text("Earnings").FontColor(Colors.White).Bold();
                                earningsTable.Cell().Border(1).BorderLeft(0).Background("#010943").Padding(3).Text("Amount").FontColor(Colors.White).Bold();

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Basic Pay").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(slip.Employee.BasicSalary.ToString("N0", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("OnTime Allowance").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(onTimeAllowance.ToString("N0", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Attendance Allowance").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(attendanceAllowance.ToString("N0", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Performance Allowance").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(performanceAllowance.ToString("N0", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Overtime").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(slip.Overtime.ToString("N0", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Commission").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(slip.Comission.HasValue ? slip.Comission.Value.ToString("N0", new CultureInfo("ur-PK")) : "N/A");

                                earningsTable.Cell().Border(1).BorderRight(0).Padding(3).Text("Gross Total").Bold();
                                earningsTable.Cell().Border(1).BorderLeft(0).Background("#D3D3D3").Padding(3).Text(grossTotal.ToString("N0", new CultureInfo("ur-PK"))).Bold();
                            });

                            table.Cell().Padding(0).Table(deductionsTable =>
                            {
                                deductionsTable.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                deductionsTable.Cell().Border(1).BorderRight(0).BorderLeft(0).Background("#F27E63").Padding(3).Text("Deductions").FontColor(Colors.White).Bold();
                                deductionsTable.Cell().Border(1).BorderLeft(0).Background("#010943").Padding(3).Text("Amount").FontColor(Colors.White).Bold();

                                deductionsTable.Cell().Padding(3).Text("Off/Late deduction").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text("(" + slip.Deductions.ToString("N0", new CultureInfo("ur-PK")) + ")");

                                if(slip.DeductionApplied == false)
                                {
                                    deductionsTable.Cell().Padding(3).Text("Deduction Adjustment").Bold();
                                    deductionsTable.Cell().BorderRight(1).Padding(3).Text(slip.Deductions.ToString("N0", new CultureInfo("ur-PK")));
                                }

                                deductionsTable.Cell().Padding(3).Text("Loan Payment").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text(slip.Loan.HasValue ? "(" + slip.Loan.Value.ToString("N0", new CultureInfo("ur-PK")) + ")": "N/A");

                                deductionsTable.Cell().Padding(3).Text("Security Deposit").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text(slip.SecurityDeposit.HasValue ? "(" + slip.SecurityDeposit.Value.ToString("N0", new CultureInfo("ur-PK")) + ")" : "N/A");

                                deductionsTable.Cell().Padding(3).Text("Income Tax").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text(slip.IncomeTax.HasValue ? "(" +slip.IncomeTax.Value.ToString("N0", new CultureInfo("ur-PK")) + ")" : "N/A");

                                if(slip.DeductionApplied == true)
                                {
                                    deductionsTable.Cell().Padding(3).Text("").Bold();
                                    deductionsTable.Cell().BorderRight(1).Padding(3).Text("");
                                }

                                deductionsTable.Cell().Padding(3).Text("").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text("");

                                deductionsTable.Cell().Border(1).BorderLeft(0).BorderRight(0).Padding(3).Text("Total Deductions").Bold();
                                deductionsTable.Cell().Border(1).BorderLeft(0).Background("#D3D3D3").Padding(3).Text("(" +  totalDeductions.ToString("N0", new CultureInfo("ur-PK")) + ")" ).Bold();
                            });
                        });

                        col.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text($"Net Pay: {slip.Total.ToString("N0", new CultureInfo("ur-PK"))}").Bold().FontSize(12);
                        });

                        col.Item().PaddingTop(100).Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("MANAGING DIRECTOR ________________________");
                        });
                    });

                    page.Footer().Height(62).Image(footer, ImageScaling.FitArea);
                });
            });
        }

        private decimal CalculateAllowances(bool hasAllowance, decimal basicSalary)
        {
            return basicSalary * 0.10m;
        }
    }
}
