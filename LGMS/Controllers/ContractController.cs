using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using LGMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace LGMS.Controllers
{
    [Authorize(Roles = "Sales")]
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<Contract> _pagedData;
        private readonly InvoicePDFService _pdfService;
        public ContractController(LgmsDbContext dbContext, InvoicePDFService pdfService)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Contract>();
            _pdfService = pdfService;
        }

        [HttpPost("GetContractsWithFilters")]
        public IActionResult GetContractsWithFilters(ContractSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var contracts = new List<Contract>();

            try
            {
                if (searchModel.ClientId != 0)
                {
                    contracts = _dbContext.Contracts
                                    .Include(c => c.ContractPackageInformations)
                                    .Include(c => c.Type)
                                    .Include(c => c.Status)
                                    .Include(c => c.Payments)
                                    .Include(c => c.Expirations)
                                    .Include(c => c.Client)
                                    .Include(c => c.DomainDetails)
                                    .Where(q => q.Client.Id == searchModel.ClientId).ToList();
                }
                else
                {
                    contracts = _dbContext.Contracts
                                    .Include(q => q.ContractPackageInformations)
                                    .Include(c => c.Type)
                                    .Include(c => c.Status)
                                    .Include(c => c.Payments)
                                    .Include(c => c.Expirations)
                                    .Include(c => c.Client)
                                    .Include(c => c.DomainDetails)
                                    .ToList();
                }
            } 
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
            if (!contracts.Any()) return NotFound(new { message = "No contracts are there" });


            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                contracts = contracts.Where(e =>
                    e.Number.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Client.Name.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Client.Number.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Type.Title.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Status.Title.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "number":
                        contracts = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    contracts.OrderBy(e => e.Number).ToList() :
                                    contracts.OrderByDescending(e => e.Number).ToList();
                        break;
                    case "startDate":
                        contracts = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    contracts.OrderBy(e => e.StartDate).ToList() :
                                    contracts.OrderByDescending(e => e.StartDate).ToList();
                        break;
                    case "type":
                        contracts = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    contracts.OrderBy(e => e.Type.Title).ToList() :
                                    contracts.OrderByDescending(e => e.Type.Title).ToList();
                        break;
                    case "status":
                        contracts = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    contracts.OrderBy(e => e.Status.Title).ToList() :
                                    contracts.OrderByDescending(e => e.Status.Title).ToList();
                        break;
                    case "expectedCompletion":
                        contracts = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    contracts.OrderBy(e => e.ExpectedCompletion).ToList() :
                                    contracts.OrderByDescending(e => e.ExpectedCompletion).ToList();
                        break;
                    default:
                        contracts = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    contracts.OrderBy(e => e.Number).ToList() :
                                    contracts.OrderByDescending(e => e.Number).ToList();
                        break;
                }
            }
            else
            {
                contracts = contracts.OrderBy(e => e.Number).ToList();
            }

            var pagedContractsResult = _pagedData.GetPagedData(
                contracts,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedContractsResult);
        }

        [HttpGet("GetContractById")]
        public IActionResult GetContractById(int id)
        {
            var contract = _dbContext.Contracts
                    .Include(c => c.Payments).ThenInclude(p => p.BankAccount)
                    .Include(c => c.ContractPackageInformations)
                    .Include(c => c.Type)
                    .Include(c => c.DomainDetails)
                    .Include(c => c.Status)
                    .Include(c => c.Expirations)
                    .Include(c => c.Client)
                    .SingleOrDefault(q => q.Id == id);
            
            if (contract == null) return BadRequest(new { message = string.Format("Contract with id {0} doesn't exist", id) });
            return Ok(contract);
        }

        [HttpPost("AddContract")]
        public IActionResult AddContract(ContractAddModel details)
        {
            var duplicatePackageTitles = details.ContractPackageInformations
                                        .GroupBy(p => p.Title.ToUpper().Trim())
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

            if (duplicatePackageTitles.Any())
            {
                return BadRequest(new { message = $"Duplicate package titles within the same contract: {string.Join(", ", duplicatePackageTitles)}" });
            }
            var client = new Client();
            if (details.Client.Id == 0)
            {
                string number = GenerateClientNumber();
                client = new Client()
                {
                    Name = details.Client.Name,
                    Number = number,
                    Phone = details.Client.Phone,
                    Email = details.Client.Email != null? details.Client.Email :null,
                    Location = details.Client.Location != null ? details.Client.Location : null,
                    Business = details.Client.Business
                };
            }
            else
            {
                client = _dbContext.Clients.Find(details.Client.Id);

                if (client == null)
                {
                    return BadRequest(new { message = "Client not found." });
                }
            }

            try
            {
                string contractNumber = GenerateContractNumber();
                Contract contract = new Contract()
                {
                    Number = contractNumber,
                    ServicesInclude = details.ServicesInclude,
                    AdditionalCharges = details.AdditionalCharges,
                    ContractAmount = details.ContractAmount,
                    Client = client,
                    ExpectedCompletion = details.ExpectedCompletion,
                    Details = details.Details,
                    StartDate = details.StartDate,
                    Status = details.Status.Id == 0
                                ? details.Status
                                : _dbContext.ContractStatuses.Single(s => s.Id == details.Status.Id),
                    CompletionDate = details.Status?.Title?.Equals("Completed", StringComparison.OrdinalIgnoreCase) == true ? DateTime.Now : null,
                    Type = details.Type.Id == 0
                            ? details.Type
                            : _dbContext.ContractTypes.Single(s => s.Id == details.Type.Id),
                    ContractPackageInformations = details.ContractPackageInformations != null ? new List<ContractPackageInformation>() : null,
                    Payments = details.Payments != null? new List<Payment>():null,
                    Expirations = details.Expirations != null ? new List<Expiration>():null
                };

                if (details.ContractPackageInformations != null && details.ContractPackageInformations.Any())
                {
                    foreach (var package in details.ContractPackageInformations)
                    {
                        contract.ContractPackageInformations.Add(new ContractPackageInformation
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
                if (details.Payments != null && details.Payments.Any())
                {
                    foreach (var pay in details.Payments)
                    {
                        var bankAccount = _dbContext.BankAccounts.SingleOrDefault(b => b.Id == pay.BankAccount.Id);
                        if (bankAccount == null)
                        {
                            return BadRequest(new { message = $"Bank Account with Id {pay.BankAccount.Id} not found." });
                        }
                        contract.Payments.Add(new Payment
                        {
                            Title = pay.Title,
                            Date = pay.Date,
                            Amount = pay.Amount,
                            BankAccount = bankAccount
                        });
                    }
                }
                if (details.Expirations != null && details.Expirations.Any())
                {
                    foreach (var exp in details.Expirations)
                    {
                        contract.Expirations.Add(new Expiration
                        {
                            Title = exp.Title,
                            Date = exp.Date
                        });
                    }
                }
                

                _dbContext.Contracts.Add(contract);
                _dbContext.SaveChanges();

                return Ok(contract);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [HttpPost("EditContract")]
        public IActionResult EditContract(ContractEditModel details)
        {
            var contract = _dbContext.Contracts
                                .Include(c => c.ContractPackageInformations)
                                .Include(c => c.Type)
                                .Include(c => c.Status)
                                .Include(c => c.Payments)
                                .Include(c => c.Expirations)
                                .Include(c => c.Client)
                                .FirstOrDefault(c => c.Id == details.Id);

            if (contract == null)
            {
                return BadRequest(new { message = "Contract not found." });
            }

            var duplicatePackageTitles = details.ContractPackageInformations
                                        .GroupBy(p => p.Title.ToUpper().Trim())
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

            if (duplicatePackageTitles.Any())
            {
                return BadRequest(new { message = $"Duplicate package titles within the same contract: {string.Join(", ", duplicatePackageTitles)}" });
            }

            var client = new Client();
            if (details.Client.Id == 0)
            {
                string number = GenerateClientNumber();
                client = new Client()
                {
                    Name = details.Client.Name,
                    Number = number,
                    Phone = details.Client.Phone,
                    Email = details.Client.Email != null ? details.Client.Email : null,
                    Location = details.Client.Location != null ? details.Client.Location : null,
                    Business = details.Client.Business
                };
            }
            else
            {
                client = _dbContext.Clients.Find(details.Client.Id);

                if (client == null)
                {
                    return BadRequest(new { message = "Client not found." });
                }
            }

            try
            {
                contract.ContractAmount = details.ContractAmount;
                contract.ServicesInclude = details.ServicesInclude;
                contract.AdditionalCharges = details.AdditionalCharges;
                contract.Client= client;
                contract.ExpectedCompletion = details.ExpectedCompletion;
                contract.StartDate = details.StartDate;
                contract.Status = details.Status.Id == 0
                                ? details.Status
                                : _dbContext.ContractStatuses.Single(s => s.Id == details.Status.Id);
                if (details.Status.Title.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    contract.CompletionDate = DateTime.Now;
                }
                else
                {
                    contract.CompletionDate = null;
                }

                contract.Type = details.Type.Id == 0
                                ? details.Type
                                : _dbContext.ContractTypes.Single(t => t.Id == details.Type.Id);
                contract.Details = details.Details;

                var incomingPackageIds = details.ContractPackageInformations.Select(p => p.Id).Where(id => id != 0 && id != null).ToList();
                var packagesToRemove = contract.ContractPackageInformations
                                                .Where(p => !incomingPackageIds.Contains(p.Id))
                                                .ToList();
                _dbContext.ContractPackagesInformation.RemoveRange(packagesToRemove);

                foreach (var package in details.ContractPackageInformations)
                {
                    if (package.Id == 0 || package.Id == null)
                    {
                        contract.ContractPackageInformations.Add(new ContractPackageInformation
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

                var incomingPaymentIds = details.Payments.Select(p => p.Id).Where(id => id != 0 && id != null).ToList();
                var paymentsToRemove = contract.Payments
                                                .Where(p => !incomingPaymentIds.Contains(p.Id))
                                                .ToList();
                _dbContext.Payments.RemoveRange(paymentsToRemove);

                foreach (var pay in details.Payments)
                {
                    var bankAccount = _dbContext.BankAccounts.SingleOrDefault(b => b.Id == pay.BankAccount.Id);
                    if (bankAccount == null)
                    {
                        return BadRequest(new { message = $"Bank Account with Id {pay.BankAccount.Id} not found." });
                    }
                    if (pay.Id == 0 || pay.Id == null)
                    {
                        contract.Payments.Add(new Payment
                        {
                            Title = pay.Title,
                            Date = pay.Date,
                            Amount = pay.Amount,
                            BankAccount = bankAccount
                        });
                    }
                }
                var incomingExpirationIds = details.Expirations.Select(e => e.Id).Where(id => id != 0 && id != null).ToList();
                var expirationsToRemove = contract.Expirations
                                            .Where(e => !incomingExpirationIds.Contains(e.Id))
                                            .ToList();
                _dbContext.Expiration.RemoveRange(expirationsToRemove);
                
                foreach (var exp in details.Expirations)
                {
                    if (exp.Id == 0 || exp.Id == null)
                    {
                        contract.Expirations.Add(new Expiration
                        {
                            Title = exp.Title,
                            Date = exp.Date
                        });
                    }
                }
                _dbContext.SaveChanges();

                return Ok(contract);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [HttpPost("GenerateInvoice")]
        public IActionResult GenerateInvoice(GenerateInvoiceModel model)
        {
            string header = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "invoice.png");
            string footer = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "footer.png");
            using (var memoryStream = new MemoryStream())
            {
                var document = _pdfService.CreateInvoicePDF(model.Contract, header, footer);
                document.GeneratePdf(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(), "application/pdf", $"{model.Contract.Number}Contract.pdf");
            }
        }

        private string GenerateContractNumber()
        {
            var lastContract = _dbContext.Contracts
                .OrderByDescending(q => q.Number)
                .FirstOrDefault();

            if (lastContract == null)
            {
                return "CN001";
            }

            var lastContractNumber = lastContract.Number;
            var numberPart = lastContractNumber.Substring(2);
            var nextNumber = (int.Parse(numberPart) + 1).ToString();
            return "CN" + nextNumber.PadLeft(Math.Max(numberPart.Length, nextNumber.Length), '0');
        }

        private string GenerateClientNumber()
        {
            var lastClient = _dbContext.Clients
                .OrderByDescending(c => c.Number)
                .FirstOrDefault();

            if (lastClient == null)
            {
                return "CLN001";
            }

            var lastClientNumber = lastClient.Number;
            var numberPart = lastClientNumber.Substring(3);
            var nextNumber = (int.Parse(numberPart) + 1).ToString();
            return "CLN" + nextNumber.PadLeft(Math.Max(numberPart.Length, nextNumber.Length), '0');
        }

    }
}
