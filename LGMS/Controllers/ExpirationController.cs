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
                expirations = _dbContext.Expiration
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!expirations.Any()) return NotFound(new { message = "Expirations Not Found" });

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                expirations = expirations.Where(e =>
                    e.Title.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())
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
