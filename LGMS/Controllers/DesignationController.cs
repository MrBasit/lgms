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
    public class DesignationController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<Designation> _pagedData;

        public DesignationController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Designation>();

        }

        [HttpGet("GetDesignations")]
        public IActionResult GetDesignations()
        {
            var designations = _dbContext.Designations.ToList();
            return Ok(designations);
        }

        [HttpPost("GetDesignationsWithFilters")]
        public IActionResult GetDesignationsWithFilters(DesignationsSearchModel designationsSearchModel)
        {
            if (designationsSearchModel == null) return BadRequest("Invalid search criteria");

            var designations = new List<Designation>();

            try
            {
                designations = _dbContext.Designations
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!designations.Any()) return NotFound("Employee status Not Found");

            if (!string.IsNullOrEmpty(designationsSearchModel.SearchDetails.SearchTerm))
            {
                designations = designations.Where(e =>
                    e.Title.ToUpper().Contains(designationsSearchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }
            if (!string.IsNullOrEmpty(designationsSearchModel.SortDetails.SortColumn) &&
                designationsSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (designationsSearchModel.SortDetails.SortColumn)
                {
                    case "id":
                        designations = designationsSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            designations.OrderBy(e => e.Id).ToList() :
                            designations.OrderByDescending(e => e.Id).ToList();
                        break;
                    case "title":
                        designations = designationsSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            designations.OrderBy(e => e.Title).ToList() :
                            designations.OrderByDescending(e => e.Title).ToList();
                        break;
                    default:
                        designations = designationsSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            designations.OrderBy(e => e.Title).ToList() :
                            designations.OrderByDescending(e => e.Title).ToList();
                        break;
                }
            }
            else
            {
                designations = designations.OrderBy(e => e.Id).ToList();
            }

            var pagedDesignationsResult = _pagedData.GetPagedData(
                designations,
                (PagedDataRequestModel)designationsSearchModel.PaginationDetails
            );

            return Ok(pagedDesignationsResult);
        }
    }
}
