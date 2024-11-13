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
    public class ContractTypeController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<ContractType> _pagedData;

        public ContractTypeController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<ContractType>();

        }

        [HttpGet("GetContractTypes")]
        public IActionResult GetContractTypes()
        {
            var types = _dbContext.ContractTypes.ToList();
            return Ok(types);
        }

        [HttpPost("GetContractTypesWithFilters")]
        public IActionResult GetContractTypesWithFilters(BaseSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var types = new List<ContractType>();

            try
            {
                types = _dbContext.ContractTypes
                                      .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!types.Any()) return NotFound(new { message = "Contract Type Not Found" });

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                types = types.Where(e =>
                    e.Title.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }
            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "id":
                        types = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            types.OrderBy(e => e.Id).ToList() :
                            types.OrderByDescending(e => e.Id).ToList();
                        break;
                    case "title":
                        types = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            types.OrderBy(e => e.Title).ToList() :
                            types.OrderByDescending(e => e.Title).ToList();
                        break;
                    default:
                        types = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            types.OrderBy(e => e.Title).ToList() :
                            types.OrderByDescending(e => e.Title).ToList();
                        break;
                }
            }
            else
            {
                types = types.OrderBy(e => e.Id).ToList();
            }

            var pagedContractTypesResult = _pagedData.GetPagedData(
                types,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedContractTypesResult);
        }

        [HttpGet("GetContractTypeById")]
        public IActionResult GetContractTypeById(int id)
        {
            var types = _dbContext.ContractTypes
                .SingleOrDefault(d => d.Id == id);
            if (types == null) return BadRequest(new { message = string.Format("Contract Type with id {0} doesn't exist", id) });
            return Ok(types);
        }

        [HttpPost("EditContractType")]
        public IActionResult EditContractType(ContractTypeEditModel TypeDetails)
        {
            var existingType = _dbContext.ContractTypes.FirstOrDefault(d => d.Id == TypeDetails.Id);

            if (existingType == null)
            {
                return NotFound(new { message = "Contract Type not Found" });
            }

            if (_dbContext.ContractTypes.Any(d => d.Title.ToUpper() == TypeDetails.Title.ToUpper() && d.Id != TypeDetails.Id))
            {
                return BadRequest(new
                {
                    message = $"Contract Type with this {TypeDetails.Title} already Exist"
                });
            }
            try
            {
                existingType.Title = TypeDetails.Title;
                _dbContext.SaveChanges();
                return Ok(existingType);
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
        [HttpPost("DeleteContractType")]
        public IActionResult DeleteContractType([FromBody] int id)
        {
            var existingType = _dbContext.ContractTypes.FirstOrDefault(d => d.Id == id);

            if (existingType == null)
            {
                return NotFound(new { message = "Contract Type not Found" });
            }
            if (_dbContext.Contracts.Any(e => e.Type.Id == existingType.Id))
            {
                return BadRequest(new { message = $"{existingType.Title} is in use and it can't be delete." });
            }

            try
            {
                _dbContext.ContractTypes.Remove(existingType);
                _dbContext.SaveChanges();
                return Ok(new { message = $"{existingType.Title} has been deleted." });
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
