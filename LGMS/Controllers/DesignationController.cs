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
            if (designationsSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });

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
            if (!designations.Any()) return NotFound(new { message = "Designation Not Found" });

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
        [HttpGet("GetDesignationById")]
        public IActionResult GetDesignationById(int id)
        {
            var designation = _dbContext.Designations
                .SingleOrDefault(d => d.Id == id);
            if (designation == null) return BadRequest(new { message = string.Format("Designation with id {0} doesn't exist", id) });
            return Ok(designation);
        }

        [HttpPost("EditDesignation")]
        public IActionResult EditDesignation(DesignationEditModel designationDetails)
        {
            var existingDesignation = _dbContext.Designations.FirstOrDefault(d => d.Id == designationDetails.Id);

            if (existingDesignation == null)
            {
                return NotFound("Designation not Found");
            }

            if (_dbContext.Designations.Any(d => d.Title.ToUpper() == designationDetails.Title.ToUpper() && d.Id != designationDetails.Id))
            {
                return BadRequest(new
                {
                    message = "Designation with this Title already Exist"
                });
            }
            try
            {
                existingDesignation.Title = designationDetails.Title;
                _dbContext.SaveChanges();
                return Ok(existingDesignation);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    innerMessage = ex.InnerException != null ? ex.InnerException.Message : ""
                });
            }
        }
        [HttpPost("DeleteDesignation")]
        public IActionResult DeleteDesignation([FromBody] int id)
        {
            var existingDesignation = _dbContext.Designations.FirstOrDefault(d => d.Id == id);

            if (existingDesignation == null)
            {
                return NotFound("Designation not Found");
            }

            try
            {
                _dbContext.Designations.Remove(existingDesignation);
                _dbContext.SaveChanges();
                return Ok(new { message = $"{existingDesignation.Title} has been deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    innerMessage = ex.InnerException != null ? ex.InnerException.Message : ""
                });
            }
        }
    }
}
