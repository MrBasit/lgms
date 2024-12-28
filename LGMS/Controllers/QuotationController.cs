using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using LGMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using QuestPDF.Fluent;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotationController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<Quotation> _pagedData;
        private readonly QuotationPDFService _pdfService;
        public QuotationController(LgmsDbContext dbContext, QuotationPDFService pdfService)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Quotation>();
            _pdfService = pdfService;
        }

        [HttpPost("GetQuotationsWithFilters")]
        public IActionResult GetQuotationsWithFilters(QuotationsSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var quotations = new List<Quotation>();

            try
            {
                quotations = _dbContext.Quotations.Include(q => q.QuotationPackageInformations).Where(q => q.Client.Id == searchModel.ClientId).ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }


            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                quotations = quotations.Where(e =>
                    e.Number.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "number":
                        quotations = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    quotations.OrderBy(e => e.Number).ToList() :
                                    quotations.OrderByDescending(e => e.Number).ToList();
                        break;
                    case "date":
                        quotations = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    quotations.OrderBy(e => e.Date).ToList() :
                                    quotations.OrderByDescending(e => e.Date).ToList();
                        break;
                    default:
                        quotations = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    quotations.OrderBy(e => e.Date).ToList() :
                                    quotations.OrderByDescending(e => e.Date).ToList();
                        break;
                }
            }
            else
            {
                quotations = quotations.OrderByDescending(e => e.Date).ToList();
            }

            var pagedQuotationsResult = _pagedData.GetPagedData(
                quotations,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedQuotationsResult);
        }

        [HttpGet("GetQuotationById")]
        public IActionResult GetQuotationById(int id)
        {
            var quotation = _dbContext.Quotations.Include(q => q.Client).Include(q => q.QuotationPackageInformations).SingleOrDefault(q => q.Id == id);
            if (quotation == null) return BadRequest(new { message = string.Format("quotation with id {0} doesn't exist", id) });
            return Ok(quotation);
        }

        [HttpPost("AddQuotation")]
        public IActionResult AddQuotation(QuotationAddModel details)
        {
            if (details.PackageInformation != null && details.PackageInformation.Any())
            {
                foreach (var package in details.PackageInformation)
                {
                    if (_dbContext.Quotations
                            .Any(q => q.QuotationPackageInformations
                            .Any(p => p.Title.ToUpper() == package.Title.ToUpper())))
                    {
                        return BadRequest(new { message = $"A Package with the title '{package.Title}' already exists for this Quotation." });
                    }
                }
            }
            var client = _dbContext.Clients.Find(details.ClientId);

            if (client == null)
            {
                return BadRequest(new { message = "Client not found." });
            }
            try
            {
                string quotationNumber = GenerateQuotationNumber(details.ClientId);
                Quotation quotation = new Quotation()
                {
                    Number = quotationNumber,
                    Date = details.Date,
                    Client = client,
                    BankAccounts = details.BankAccounts ?? new List<BankAccount>(),
                    QuotationPackageInformations = new List<QuotationPackageInformation>()
                };

                if (details.PackageInformation != null && details.PackageInformation.Any())
                {
                    foreach (var package in details.PackageInformation)
                    {
                        quotation.QuotationPackageInformations.Add(new QuotationPackageInformation
                        {
                            Title = package.Title,
                            Price = package.Price,
                            Quantity = package.Quantity,
                            Discount = package.Discount,
                            Total = package.Total,
                            Description = package.Description != null? package.Description : null
                        });
                    }
                }

                _dbContext.Quotations.Add(quotation);
                _dbContext.SaveChanges();

                return Ok (quotation);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [HttpPost("EditQuotation")]
        public IActionResult EditQuotation(QuotationEditModel details)
        {
            var quotation = _dbContext.Quotations
                                      .Include(q => q.Client)
                                      .Include(q => q.QuotationPackageInformations)
                                      .FirstOrDefault(q => q.Id == details.Id);

            if (quotation == null)
            {
                return BadRequest(new { message = "Quotation not found." });
            }

            if (details.PackageInformation != null && details.PackageInformation.Any())
            {
                foreach (var package in details.PackageInformation)
                {
                    if (_dbContext.Quotations
                        .Any(q => q.Id != details.Id && q.QuotationPackageInformations.Any(p => p.Title.ToUpper() == package.Title.ToUpper())))
                    {
                        return BadRequest(new { message = $"A Package with the title '{package.Title}' already exists for this Quotation." });
                    }
                }
            }

            try
            {
                quotation.Date = details.Date;
                quotation.BankAccounts = details.BankAccounts;

                var incomingPackageIds = details.PackageInformation.Select(p => p.Id).Where(id => id != 0 && id != null).ToList();

                var packagesToRemove = quotation.QuotationPackageInformations
                                                .Where(p => !incomingPackageIds.Contains(p.Id))
                                                .ToList();
                _dbContext.QuotationPackagesInformation.RemoveRange(packagesToRemove);

                foreach (var package in details.PackageInformation)
                {
                    if (package.Id == 0 || package.Id == null)
                    {
                        quotation.QuotationPackageInformations.Add(new QuotationPackageInformation
                        {
                            Title = package.Title,
                            Price = package.Price,
                            Quantity = package.Quantity,
                            Discount = package.Discount,
                            Total = package.Total,
                            Description = package.Description != null ? package.Description : null
                        });
                    }
                }

                _dbContext.SaveChanges();

                return Ok(quotation);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [HttpPost("GenerateQuotation")]
        public IActionResult GenerateQuotation(GenerateQuotationModel model)
        {
            string header = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "quotationheader.png");
            string footer = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "quotationfooter.png");
            using (var memoryStream = new MemoryStream())
            {
                var document = _pdfService.CreateQuotationPDF(model.Quotation, model.BankAccounts, header, footer);
                document.GeneratePdf(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/pdf", $"{model.Quotation.Number}_Quotation.pdf");
            }
        }

        private string GenerateQuotationNumber(int clientId)
        {
            var lastQuotation = _dbContext.Quotations
                .Where(q => q.Client.Id == clientId)
                .OrderByDescending(q => q.Number)
                .FirstOrDefault();

            if (lastQuotation == null)
            {
                return "QN01";
            }

            var lastQuotationNumber = lastQuotation.Number;
            var numberPart = lastQuotationNumber.Substring(2);
            var nextNumber = (int.Parse(numberPart) + 1).ToString();
            return "QN" + nextNumber.PadLeft(Math.Max(numberPart.Length, nextNumber.Length), '0');
        }

    }
}
