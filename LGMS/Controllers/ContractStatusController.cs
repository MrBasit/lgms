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
    public class ContractStatusController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<ContractStatus> _pagedData;

        public ContractStatusController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<ContractStatus>();

        }

        [HttpGet("GetContractStatuses")]
        public IActionResult GetContractStatuses()
        {
            var statuses = _dbContext.ContractStatuses.ToList();
            return Ok(statuses);
        }

        [HttpPost("GetContractStatusesWithFilters")]
        public IActionResult GetContractStatusesWithFilters(BaseSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var statuses = new List<ContractStatus>();

            try
            {
                statuses = _dbContext.ContractStatuses
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
            if (!statuses.Any()) return NotFound(new { message = "Contract status Not Found" });

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                statuses = statuses.Where(e =>
                    e.Title.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }
            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "id":
                        statuses = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            statuses.OrderBy(e => e.Id).ToList() :
                            statuses.OrderByDescending(e => e.Id).ToList();
                        break;
                    case "title":
                        statuses = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            statuses.OrderBy(e => e.Title).ToList() :
                            statuses.OrderByDescending(e => e.Title).ToList();
                        break;
                    default:
                        statuses = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            statuses.OrderBy(e => e.Title).ToList() :
                            statuses.OrderByDescending(e => e.Title).ToList();
                        break;
                }
            }
            else
            {
                statuses = statuses.OrderBy(e => e.Id).ToList();
            }

            var pagedContractStatusesResult = _pagedData.GetPagedData(
                statuses,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedContractStatusesResult);
        }
        [HttpGet("GetContractStatusById")]
        public IActionResult GetContractStatusById(int id)
        {
            var status = _dbContext.ContractStatuses
                .SingleOrDefault(d => d.Id == id);
            if (status == null) return BadRequest(new { message = string.Format("Contract Status with id {0} doesn't exist", id) });
            return Ok(status);
        }

        [HttpPost("EditContractStatus")]
        public IActionResult EditContractStatus(ContractStatusEditModel statusDetails)
        {
            var existingStatus = _dbContext.ContractStatuses.FirstOrDefault(d => d.Id == statusDetails.Id);

            if (existingStatus == null)
            {
                return NotFound(new { message = "Contract Status not Found" });
            }

            if (_dbContext.ContractStatuses.Any(d => d.Title.ToUpper() == statusDetails.Title.ToUpper() && d.Id != statusDetails.Id))
            {
                return BadRequest(new
                {
                    message = $"Contract Status with this {statusDetails.Title} already Exist"
                });
            }
            try
            {
                existingStatus.Title = statusDetails.Title;
                _dbContext.SaveChanges();
                return Ok(existingStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }
        [HttpPost("DeleteContractStatus")]
        public IActionResult DeleteContractStatus([FromBody] int id)
        {
            var existingStatus = _dbContext.ContractStatuses.FirstOrDefault(d => d.Id == id);

            if (existingStatus == null)
            {
                return NotFound(new { message = "Contract Status not Found" });
            }
            if (_dbContext.Contracts.Any(e => e.Status.Id == existingStatus.Id))
            {
                return BadRequest(new { message = $"{existingStatus.Title} is in use and it can't be delete." });
            }

            try
            {
                _dbContext.ContractStatuses.Remove(existingStatus);
                _dbContext.SaveChanges();
                return Ok(new { message = $"{existingStatus.Title} has been deleted." });
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
