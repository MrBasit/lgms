using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
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
                payments = _dbContext.Payments
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!payments.Any()) return NotFound(new { message = "Payments Not Found" });

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                payments = payments.Where(e =>
                    e.Title.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())
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
                            payments.OrderBy(e => e.BankAccount).ToList() :
                            payments.OrderByDescending(e => e.BankAccount).ToList();
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
