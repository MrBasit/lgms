using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpirationController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<Expiration> _pagedData;

        public ExpirationController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Expiration>();

        }

        [HttpPost("GetExpirationsWithFilters")]
        public IActionResult GetExpirationsWithFilters(BaseSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var expirations = new List<Expiration>();

            try
            {
                expirations = _dbContext.Expiration.Include(e => e.Contract).ThenInclude(c => c.Client)
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
            if (!expirations.Any()) return NotFound(new { message = "Expirations Not Found" });

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                expirations = expirations.Where(e =>
                    e.Title.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
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
                        expirations = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            expirations.OrderBy(e => e.Title).ToList() :
                            expirations.OrderByDescending(e => e.Title).ToList();
                        break;
                    case "date":
                        expirations = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            expirations.OrderBy(e => e.Date).ToList() :
                            expirations.OrderByDescending(e => e.Date).ToList();
                        break;
                    case "contract":
                        expirations = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            expirations.OrderBy(e => e.Contract.Number).ToList() :
                            expirations.OrderByDescending(e => e.Contract.Number).ToList();
                        break;
                    default:
                        expirations = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            expirations.OrderBy(e => e.Date).ToList() :
                            expirations.OrderByDescending(e => e.Date).ToList();
                        break;
                }
            }
            else
            {
                expirations = expirations.OrderBy(e => e.Date).ToList();
            }

            var pagedExpirationsResult = _pagedData.GetPagedData(
                expirations,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedExpirationsResult);
        }
    }
}
