using LGMS.Dto;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System.Globalization;
using QuestPDF.Fluent;
using LGMS.Data.Model;

namespace LGMS.Services
{
    public class InvoicePDFService
    {
        public IDocument CreateInvoicePDF(ContractDTO contract, string header, string footer)
        {
            var totalAmount = contract.ContractPackageInformations.Count > 0 ? contract.ContractPackageInformations.Sum(p => p.Total) : 0;
            var chargesPaid = contract.Payments.Count > 0 ? contract.Payments.Sum(p => p.Amount) : 0;
            var latestpayment = contract.Payments.Count > 0 ? contract.Payments.OrderByDescending(p => p.Date).FirstOrDefault() : null;
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Height(110).AlignCenter().Image(header, ImageScaling.FitArea);

                    page.Content().PaddingRight(80).PaddingLeft(80).Column(col =>
                    {
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.RelativeColumn(100);
                                columns.ConstantColumn(110);
                                columns.RelativeColumn(50);
                            });
                            table.Cell().PaddingBottom(3).Text("Currency: ").Bold();
                            table.Cell().PaddingBottom(3).Text("PKR");
                            table.Cell().PaddingBottom(3).Text("Contract Number: ").Bold();
                            table.Cell().PaddingBottom(3).Text(contract.Number);

                            table.Cell().PaddingBottom(3).Text("Name: ").Bold();
                            table.Cell().PaddingBottom(3).Text(contract.Client.Name).FontColor("#010943");
                            table.Cell().PaddingBottom(3).Text("Address: ").Bold();
                            table.Cell().PaddingBottom(3).Text(contract.Client.Location != null ? contract.Client.Location : "-");

                            table.Cell().PaddingBottom(3).Text("Contact: ").Bold();
                            table.Cell().PaddingBottom(3).Text(contract.Client.Phone);
                            table.Cell().PaddingBottom(3).Text("Email: ").Bold();
                            table.Cell().PaddingBottom(3).Text(contract.Client.Email != null ? contract.Client.Email : "-");
                        });

                        if (contract.ContractPackageInformations.Count > 0)
                        {
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                table.Cell().Border(1).Background("#F27E63").Padding(3).Text("Product Name").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Background("#010943").Padding(3).Text("Discount").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Background("#F27E63").Padding(3).Text("Quantity").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Background("#010943").Padding(3).Text("Price").FontColor(Colors.White).Bold();

                                foreach (var package in contract.ContractPackageInformations)
                                {
                                    table.Cell().Border(1).Padding(3).Text(package.Title);
                                    table.Cell().Border(1).Padding(3).Text(package.Discount + "%");
                                    table.Cell().Border(1).Padding(3).Text(package.Quantity);
                                    table.Cell().Border(1).Padding(3).Text(package.Total.ToString("N0"));
                                }
                            });
                            col.Item().Padding(0).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                table.Cell().Border(1).Padding(3).AlignCenter().Text("Total").Bold();
                                table.Cell().Border(1).Background("#D3D3D3").Padding(3).AlignCenter().Text(totalAmount.ToString("N0"));
                            });
                            col.Item().Padding(0).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                table.Cell().Border(1).Padding(3).AlignCenter().Text("Charges Paid").Bold();
                                table.Cell().Border(1).Background("#D3D3D3").Padding(3).AlignCenter().Text("(" + chargesPaid.ToString("N0") + ")");
                            });
                            col.Item().Padding(0).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                table.Cell().Border(1).Padding(3).AlignCenter().Text("Remaining Payment").Bold();
                                table.Cell().Border(1).Background("#D3D3D3").Padding(3).AlignCenter().Text((totalAmount - chargesPaid).ToString("N0"));
                            });
                        }
                        if (!string.IsNullOrEmpty(contract.ServicesInclude))
                        {
                            col.Item().PaddingTop(10).Text("Services Include: ").Bold().FontSize(14).FontColor(Colors.Black);
                            var services = ParseServicesInclude(contract.ServicesInclude);
                            col.Item().Padding(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                table.Cell().Border(1).Background("#F27E63").Padding(3).AlignCenter().Text("Features").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Background("#010943").Padding(3).AlignCenter().Text("Includes").FontColor(Colors.White).Bold();

                                foreach (var service in services)
                                {
                                    table.Cell().Border(1).Padding(3).Text(service.Key);
                                    table.Cell().Border(1).Padding(3).Text(service.Value);
                                }
                            });
                        }

                        col.Item().PaddingTop(10).Text("Additional Charges: ").Bold().FontSize(14).FontColor(Colors.Black);

                        col.Item().Padding(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Cell().Border(1).Background("#F27E63").Padding(3).AlignCenter().Text("Additional Charges").FontColor(Colors.White).Bold();
                            table.Cell().Border(1).Background("#010943").Padding(3).AlignCenter().Text("Charges").FontColor(Colors.White).Bold();
                            table.Cell().Border(1).Padding(3).Text("Additional product add");
                            table.Cell().Border(1).Padding(3).Text("50");
                            table.Cell().Border(1).Padding(3).Text("Additional Banner design");
                            table.Cell().Border(1).Padding(3).Text("200");
                            table.Cell().Border(1).Padding(3).Text("Additional logo concept");
                            table.Cell().Border(1).Padding(3).Text("1000");
                            table.Cell().Border(1).Padding(3).Text("Additional Page");
                            table.Cell().Border(1).Padding(3).Text("1,500");
                            table.Cell().Border(1).Padding(3).Text("Additional Payment method");
                            table.Cell().Border(1).Padding(3).Text("1,000");
                            table.Cell().Border(1).Padding(3).Text("Domain Charges (.shop, .store, .site, .online)");
                            table.Cell().Border(1).Padding(3).Text("1,000");
                            table.Cell().Border(1).Padding(3).Text("Domain Charges (.com)");
                            table.Cell().Border(1).Padding(3).Text("3,000");
                            table.Cell().Border(1).Padding(3).Text("Content writing ChatGPT 4 with (Human touch)");
                            table.Cell().Border(1).Padding(3).Text("Per word 3 Rupees");
                            table.Cell().Border(1).Padding(3).Text("Content writing ChatGPT 4 (SEO content with keyword)");
                            table.Cell().Border(1).Padding(3).Text("Per word 4 Rupees");

                        });

                        col.Item().PaddingTop(10)
                                    .Text(text =>
                                    {
                                        text.Span("Expected Compeletion Date: ").Bold().FontSize(14).FontColor(Colors.Black);
                                        text.Span(contract.ExpectedCompletion.ToString("yyyy-MM-dd")).FontSize(14).FontColor(Colors.Black);
                                    });

                        col.Item().PaddingTop(10).Text("After Sale Information: ").Bold().FontSize(14).FontColor(Colors.Black);

                        if (contract.Status.Title == "Completed" && contract.Payments.Count > 0)
                        {
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                table.Cell().Border(1).Background("#F27E63").Padding(3).AlignCenter().Text("Title").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Background("#010943").Padding(3).AlignCenter().Text("Amount").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Background("#F27E63").Padding(3).AlignCenter().Text("Date").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Padding(3).AlignCenter().Text(latestpayment.Title).Bold();
                                table.Cell().Border(1).Padding(3).AlignCenter().Text(latestpayment.Amount.ToString("N0")).Bold();
                                table.Cell().Border(1).Padding(3).AlignCenter().Text(latestpayment.Date.ToString("yyyy-MM-dd")).Bold();
                            });
                        }

                        if (contract.Expirations.Count > 0)
                        {
                            col.Item().PaddingTop(10).Text("Expiration of ongoing services:").Bold().FontSize(14).FontColor(Colors.Black);
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });
                                table.Cell().Border(1).Background("#F27E63").Padding(3).AlignCenter().Text("Services").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Background("#010943").Padding(3).AlignCenter().Text("Expiry Date").FontColor(Colors.White).Bold();
                                foreach (var exp in contract.Expirations)
                                {
                                    table.Cell().Border(1).Padding(3).Text(exp.Title).Bold();
                                    table.Cell().Border(1).Padding(3).Text(exp.Date.ToString("yyyy-MM-dd")).Bold();
                                }
                            });
                        }
                    });

                    page.Footer().Height(62).Image(footer, ImageScaling.FitArea);
                });
            });
        }

        private static Dictionary<string, string> ParseServicesInclude(string input)
        {
            var result = new Dictionary<string, string>();

            var lines = input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ':' }, 2); 
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim(); 
                    var value = parts[1].Trim();
                    result[key] = value;
                }
            }

            return result;
        }

    }
}
