using LGMS.Dto;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System.Globalization;
using QuestPDF.Fluent;


namespace LGMS.Services
{
    public class SalarySlipPDFService
    {
        public IDocument CreateSalarySlipPdf(SalarySlipDTO slip, string logoPath)
        {
            decimal basicSalary = slip.Salary;
            decimal onTimeAllowance = slip.OnTimeAllowance? CalculateAllowances(true, basicSalary): 0;
            decimal attendanceAllowance = slip.AttendanceAllowance? CalculateAllowances(slip.AttendanceAllowance, basicSalary) : 0;
            decimal performanceAllowance = slip.PerformanceAllowance.HasValue && slip.PerformanceAllowance.Value? CalculateAllowances(true, basicSalary): 0;
            decimal grossTotal = basicSalary + onTimeAllowance + attendanceAllowance + performanceAllowance + slip.Overtime + (slip.Comission ?? 0);
            decimal totalDeductions = slip.Deductions;
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
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().AlignCenter().Height(80).Image(logoPath, ImageScaling.FitHeight);

                    page.Content().PaddingVertical(10).Column(col =>
                    {

                        col.Item().AlignCenter().Text("SALARY SLIP").Bold().FontSize(16).FontColor(Colors.Black);

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(150);
                                columns.RelativeColumn();
                                columns.ConstantColumn(150);
                                columns.RelativeColumn();
                            });

                            table.Cell().Padding(3).Text("Genrated Date:").Bold();
                            table.Cell().Padding(3).Text(DateTime.Now.ToString("yyyy-MM-dd"));
                            table.Cell().Padding(3).Text("Employee Name:").Bold();
                            table.Cell().Padding(3).Text(slip.Name);

                            table.Cell().Padding(3).Text("Pay Period:").Bold();
                            table.Cell().Padding(3).Text(slip.PayPeriod.Value.ToString("MMM yyyy"));
                            table.Cell().Padding(3).Text("Designation:").Bold();
                            table.Cell().Padding(3).Text(slip.Designation);

                            table.Cell().Padding(3).Text("Currency:").Bold();
                            table.Cell().Padding(3).Text("PKR");
                            table.Cell().Padding(3).Text("Department:").Bold();
                            table.Cell().Padding(3).Text(slip.Department);
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

                                earningsTable.Cell().Border(1).BorderRight(0).Padding(3).Text("Earnings").Bold();
                                earningsTable.Cell().Border(1).BorderLeft(0).Padding(3).Text("Amount").Bold();

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Basic Pay").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(slip.Salary.ToString("C", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("OnTime Allowance").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(onTimeAllowance.ToString("C", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Attendance Allowance").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(attendanceAllowance.ToString("C", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Performance Allowance").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(performanceAllowance.ToString("C", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Overtime").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(slip.Overtime.ToString("C", new CultureInfo("ur-PK")));

                                earningsTable.Cell().BorderLeft(1).Padding(3).Text("Commission").Bold();
                                earningsTable.Cell().BorderRight(1).Padding(3).Text(slip.Comission.HasValue ? slip.Comission.Value.ToString("C", new CultureInfo("ur-PK")) : "N/A");

                                earningsTable.Cell().Border(1).BorderRight(0).Padding(3).Text("Gross Total").Bold();
                                earningsTable.Cell().Border(1).BorderLeft(0).Padding(3).Text(grossTotal.ToString("C", new CultureInfo("ur-PK"))).Bold();
                            });

                            table.Cell().Padding(0).Table(deductionsTable =>
                            {
                                deductionsTable.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                deductionsTable.Cell().Border(1).BorderRight(0).BorderLeft(0).Padding(3).Text("Deductions").Bold();
                                deductionsTable.Cell().Border(1).BorderLeft(0).Padding(3).Text("Amount").Bold();

                                deductionsTable.Cell().Padding(3).Text("Off/Late deduction").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text(slip.Deductions.ToString("C", new CultureInfo("ur-PK")));

                                deductionsTable.Cell().Padding(3).Text("Loan").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text(slip.Loan.HasValue ?slip.Loan.Value.ToString("C", new CultureInfo("ur-PK")) : "N/A");

                                deductionsTable.Cell().Padding(3).Text("Security Deposit").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text(slip.SecurityDeposit.HasValue ? slip.SecurityDeposit.Value.ToString("C", new CultureInfo("ur-PK")) : "N/A");

                                deductionsTable.Cell().Padding(3).Text("Income Tax").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text(slip.IncomeTax.HasValue ? slip.IncomeTax.Value.ToString("C", new CultureInfo("ur-PK")) : "N/A");

                                deductionsTable.Cell().Padding(3).Text("").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text("");

                                deductionsTable.Cell().Padding(3).Text("").Bold();
                                deductionsTable.Cell().BorderRight(1).Padding(3).Text("");

                                deductionsTable.Cell().Border(1).BorderLeft(0).BorderRight(0).Padding(3).Text("Total Deductions").Bold();
                                deductionsTable.Cell().Border(1).BorderLeft(0).Padding(3).Text(totalDeductions.ToString("C", new CultureInfo("ur-PK"))).Bold();
                            });
                        });

                        col.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text($"Net Pay: {slip.Total.ToString("C", new CultureInfo("ur-PK"))}").Bold().FontSize(12);
                        });

                        col.Item().PaddingTop(100).Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("DIRECTOR ________________________");
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });
        }
        private decimal CalculateAllowances(bool hasAllowance, decimal basicSalary)
        {
            return basicSalary * 0.10m;
        }
    }
}
