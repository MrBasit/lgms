using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DomainDetailsController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<DomainDetails> _pagedData;
        public DomainDetailsController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<DomainDetails>();
        }

        [HttpPost("GetDomainsWithFilters")]
        public IActionResult GetDomainsWithFilters(DomainDetailsSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new {message = "Invalid search criteria"});

            var domains = new List<DomainDetails>();
            try
            {
                if (searchModel.ContractId != 0)
                {
                    domains = _dbContext.DomainDetails.Include(d => d.Contract).ThenInclude(c => c.Client)
                        .Where(d => d.ContractId == searchModel.ContractId)
                        .ToList();
                }
                else
                {
                    domains = _dbContext.DomainDetails.Include(d => d.Contract).ThenInclude(c => c.Client).ToList();
                }
            }
            catch (Exception ex) 
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
            if (!domains.Any()) return NotFound(new { message = "No domain details are there" });

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                domains = domains.Where(e =>
                    e.Contract.Number.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Contract.Client.Name.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Contract.Client.Number.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.DomainName.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) 
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "name":
                        domains = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    domains.OrderBy(e => e.DomainName).ToList() :
                                    domains.OrderByDescending(e => e.DomainName).ToList();
                        break;
                    case "contract":
                        domains = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    domains.OrderBy(e => e.Contract.Number).ToList() :
                                    domains.OrderByDescending(e => e.Contract.Number).ToList();
                        break;
                    case "expirationDate":
                        domains = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    domains.OrderBy(e => e.ExpirationDate).ToList() :
                                    domains.OrderByDescending(e => e.ExpirationDate).ToList();
                        break;
                    default:
                        domains = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    domains.OrderBy(e => e.DomainName).ToList() :
                                    domains.OrderByDescending(e => e.DomainName).ToList();
                        break;
                }
            }
            else
            {
                domains = domains.OrderBy(e => e.DomainName).ToList();
            }

            var pagedDomainsResult = _pagedData.GetPagedData(
                domains,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedDomainsResult);
        }

        [HttpGet("GetDomainDetailById")]
        public IActionResult GetDomainDetailById(int id)
        {
            var domain = _dbContext.DomainDetails.Include(d => d.Contract).FirstOrDefault(d => d.Id == id);
            if (domain == null) return BadRequest(new {message = $"Domain details with this id{id} not found."});
            return Ok(domain);
        }

        [HttpPost("AddDomainDetail")]
        public IActionResult AddDomainDetail(DomainDetailsAddModel details)
        {
            if(_dbContext.DomainDetails.Any(d=> d.DomainName == details.DomainName))
            {
                return BadRequest(new { message = $"A domain with this name {details.DomainName} already exists." });
            }
            var contract = _dbContext.Contracts.SingleOrDefault(c => c.Id == details.ContractId);
            if (contract == null) return NotFound(new {message = "Contract not found."});
            try
            {
                DomainDetails domain = new DomainDetails()
                {
                    DomainName = details.DomainName,
                    RegistrationDate = details.RegistrationDate,
                    ExpirationDate = details.ExpirationDate,
                    FirstName = details.FirstName,
                    LastName = details.LastName,
                    CompanyName = details.CompanyName,
                    Email = details.Email,
                    Province = details.Province,
                    City = details.City,
                    PostalCode = details.PostalCode,
                    Address = details.Address,
                    PhoneNumber = details.PhoneNumber,
                    IsExistingDomain = details.IsExistingDomain,
                    IsOwned = details.IsOwned,
                    Platform = details.Platform,
                    PlatformAccountName = details.PlatformAccountName,
                    TransferredTo = details.TransferredTo,
                    TransferredDate = details.TransferredDate,
                    ContractId = details.ContractId,
                    Contract = contract,
                };
                _dbContext.DomainDetails.Add(domain);
                _dbContext.SaveChanges();

                return Ok(domain);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [HttpPost("EditDomainDetail")]
        public IActionResult EditDomainDetail(DomainDetailsEditModel details)
        {
            var domain = _dbContext.DomainDetails
                                .Include(c => c.Contract)
                                .FirstOrDefault(c => c.Id == details.Id);

            if (domain == null) return BadRequest(new {message = $"No domain detail found with this id {details.Id}"});

            if (_dbContext.DomainDetails.Any(d => d.DomainName == details.DomainName && d.Id != details.Id))
            {
                return BadRequest(new { message = $"A domain with this name {details.DomainName} already exists." });
            }

            var contract = _dbContext.Contracts.SingleOrDefault(c => c.Id == details.ContractId);
            if (contract == null) return NotFound(new { message = "Contract not found." });

            try
            {
                domain.DomainName = details.DomainName;
                domain.RegistrationDate = details.RegistrationDate;
                domain.ExpirationDate = details.ExpirationDate;
                domain.FirstName = details.FirstName;
                domain.LastName = details.LastName;
                domain.CompanyName = details.CompanyName;
                domain.Email = details.Email;
                domain.Province = details.Province;
                domain.City = details.City;
                domain.PostalCode = details.PostalCode;
                domain.Address = details.Address;
                domain.PhoneNumber = details.PhoneNumber;
                domain.IsExistingDomain = details.IsExistingDomain;
                domain.IsOwned = details.IsOwned;
                domain.Platform = details.Platform;
                domain.PlatformAccountName = details.PlatformAccountName;
                domain.TransferredTo = details.TransferredTo;
                domain.TransferredDate = details.TransferredDate;
                domain.ContractId = details.ContractId;
                domain.Contract = contract;

                _dbContext.SaveChanges();

                return Ok(domain);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

    }
}
