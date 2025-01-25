using LGMS.Dto;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System.Globalization;
using QuestPDF.Fluent;
using LGMS.Data.Model;

namespace LGMS.Services
{
    public class QuotationPDFService
    {

        public IDocument CreateQuotationPDF(QuotationDTO quotation, List<BankAccountDTO> accounts, string header, string footer)
        {
            var totalAmount = quotation.QuotationPackageInformations.Count > 0 ? quotation.QuotationPackageInformations.Sum(p => p.Total) : 0;
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
                            table.Cell().PaddingBottom(3).Text("Quotation Number: ").Bold();
                            table.Cell().PaddingBottom(3).Text(quotation.Number);

                            table.Cell().PaddingBottom(3).Text("Name: ").Bold();
                            table.Cell().PaddingBottom(3).Text(quotation.Client.Name).FontColor("#010943");
                            table.Cell().PaddingBottom(3).Text("Address: ").Bold();
                            table.Cell().PaddingBottom(3).Text(quotation.Client.Location != null ? quotation.Client.Location : "-");

                            table.Cell().PaddingBottom(3).Text("Contact: ").Bold();
                            table.Cell().PaddingBottom(3).Text(quotation.Client.Phone);
                            table.Cell().PaddingBottom(3).Text("Email: ").Bold();
                            table.Cell().PaddingBottom(3).Text(quotation.Client.Email != null ? quotation.Client.Email : "-");
                        });

                        if (quotation.QuotationPackageInformations.Count > 0)
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
                                table.Cell().Border(1).Background("#010943").Padding(3).Text("Discount Amount").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Background("#F27E63").Padding(3).Text("Quantity").FontColor(Colors.White).Bold();
                                table.Cell().Border(1).Background("#010943").Padding(3).Text("Price").FontColor(Colors.White).Bold();

                                foreach (var package in quotation.QuotationPackageInformations)
                                {
                                    table.Cell().Border(1).Padding(3).Text(package.Title);
                                    table.Cell().Border(1).Padding(3).Text(package.Discount).FontColor("#FF0000");
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
                        }

                        col.Item().PaddingTop(10).Text("From").Bold().FontSize(14).FontColor(Colors.Black);

                        col.Item().PaddingTop(10).BorderBottom(1).PaddingBottom(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.RelativeColumn(200);
                                columns.ConstantColumn(60);
                                columns.RelativeColumn(200);
                            });
                            table.Cell().Text("Company: ").Bold();
                            table.Cell().Text("Logicade.io");
                            table.Cell().Text("Address: ").Bold();
                            table.Cell().Text("A-1734 Gulshan e Hadeed Phase 2, Bin Qasim Town Karachi.");

                            table.Cell().Text("Contact: ").Bold();
                            table.Cell().Text("+92 317 8558005");
                            table.Cell().Text("Email: ").Bold();
                            table.Cell().Text("info@logicade.io");
                        });

                        if (accounts.Count > 0 && accounts != null)
                        {
                            foreach (var account in accounts)
                            {
                                col.Item().PaddingTop(5)
                                    .Text(text =>
                                    {
                                        text.Span("BankName: ").Bold().FontSize(14).FontColor(Colors.Black);
                                        text.Span(account.BankName).FontSize(14).FontColor(Colors.Black);
                                    });

                                col.Item().PaddingTop(5).PaddingBottom(3)
                                    .Text(text =>
                                    {
                                        text.Span("AccountTitle: ").Bold();
                                        text.Span(account.AccountTitle);
                                    });

                                col.Item().PaddingBottom(3)
                                    .Text(text =>
                                    {
                                        text.Span("AccountNumber: ").Bold();
                                        text.Span(account.AccountNumber);
                                    });

                                if (!string.IsNullOrEmpty(account.IBAN))
                                {
                                    col.Item()
                                        .Text(text =>
                                        {
                                            text.Span("IBAN: ").Bold();
                                            text.Span(account.IBAN);
                                        });
                                }
                            }
                        }
                    });

                    page.Footer().Height(62).Image(footer, ImageScaling.FitArea); 
                });
            });
        }

    }
}
