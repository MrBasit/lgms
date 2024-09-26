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
    public class ManufacturerController : ControllerBase
    {
        private LgmsDbContext _dbContext;
        private PagedData<Manufacturer> _pagedData;

        public ManufacturerController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Manufacturer>();
        }

        [HttpGet("GetManufacturers")]
        public IActionResult GetManufacturers()
        {
            var manufacturers = _dbContext.Manufacturers.ToList();

            return Ok(manufacturers);
        }

        [HttpPost("GetManufacturersWithFilters")]
        public IActionResult GetManufacturersWithFilters(ManufacturersSearchModel manufacturerSearchModel)
        {
            if (manufacturerSearchModel == null) return BadRequest("Invalid search criteria");
            var manufacturers = new List<Manufacturer>();
            try
            {
                manufacturers = _dbContext.Manufacturers.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (!manufacturers.Any()) return NotFound("Manufacturer Not Found");

            if (!string.IsNullOrEmpty(manufacturerSearchModel.SearchDetails.SearchTerm))
            {
                manufacturers = manufacturers.Where(v =>
                    v.Name.ToUpper().Contains(manufacturerSearchModel.SearchDetails.SearchTerm.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(manufacturerSearchModel.SortDetails.SortColumn) && manufacturerSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (manufacturerSearchModel.SortDetails.SortColumn)
                {
                    case "id":
                        manufacturers = manufacturerSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            manufacturers.OrderBy(e => e.Id).ToList() :
                            manufacturers.OrderByDescending(e => e.Id).ToList();
                        break;
                    case "name":
                        manufacturers = manufacturerSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            manufacturers.OrderBy(e => e.Name).ToList() :
                            manufacturers.OrderByDescending(e => e.Name).ToList();
                        break;
                    default:
                        manufacturers = manufacturerSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            manufacturers.OrderBy(e => e.Name).ToList() :
                            manufacturers.OrderByDescending(e => e.Name).ToList();
                        break;
                }
            }
            else
            {
                manufacturers = manufacturers.OrderBy(e => e.Id).ToList();
            }
            var pagedManufacturersResult = _pagedData.GetPagedData(
                manufacturers,
                (PagedDataRequestModel)manufacturerSearchModel.PaginationDetails
            );

            return Ok(pagedManufacturersResult);
        }
    }
}
