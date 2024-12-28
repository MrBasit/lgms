using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Authorize(Roles = "Sales")]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<Payment> _pagedData;

        public PaymentsController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Payment>();

        }

        [HttpPost("GetPaymentsWithFilters")]
        public IActionResult GetPaymentsWithFilters(BaseSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var payments = new List<Payment>();

            try
            {
                payments = _dbContext.Payments.Include(p => p.BankAccount).Include(p => p.Contract).ThenInclude(c => c.Client)
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
            if (!payments.Any()) return NotFound(new { message = "Payments Not Found" });

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                payments = payments.Where(e =>
                    e.Title.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.BankAccount.AccountTitle.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.BankAccount.BankName.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Contract.Client.Name.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Contract.Client.Number.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Contract.Number.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }
            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "title":
                        payments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            payments.OrderBy(e => e.Title).ToList() :
                            payments.OrderByDescending(e => e.Title).ToList();
                        break;
                    case "date":
                        payments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            payments.OrderBy(e => e.Date).ToList() :
                            payments.OrderByDescending(e => e.Date).ToList();
                        break;
                    case "amount":
                        payments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            payments.OrderBy(e => e.Amount).ToList() :
                            payments.OrderByDescending(e => e.Amount).ToList();
                        break;
                    case "bankAccount":
                        payments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            payments.OrderBy(e => e.BankAccount.AccountTitle).ToList() :
                            payments.OrderByDescending(e => e.BankAccount.AccountTitle).ToList();
                        break;
                    case "contract":
                        payments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            payments.OrderBy(e => e.Contract.Number).ToList() :
                            payments.OrderByDescending(e => e.Contract.Number).ToList();
                        break;
                    default:
                        payments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            payments.OrderBy(e => e.Date).ToList() :
                            payments.OrderByDescending(e => e.Date).ToList();
                        break;
                }
            }
            else
            {
                payments = payments.OrderBy(e => e.Date).ToList();
            }

            var pagedPaymentTypesResult = _pagedData.GetPagedData(
                payments,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedPaymentTypesResult);
        }
    }
}
