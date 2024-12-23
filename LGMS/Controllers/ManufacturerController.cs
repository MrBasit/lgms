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
        public IActionResult GetManufacturersWithFilters(BaseSearchModel manufacturerSearchModel)
        {
            if (manufacturerSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });
            var manufacturers = new List<Manufacturer>();
            try
            {
                manufacturers = _dbContext.Manufacturers.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }

            if (!manufacturers.Any()) return NotFound(new { message = "No manufacturers are there" });

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
        [HttpGet("GetManufacturerById")]
        public IActionResult GetManufacturerById(int id)
        {
            var manufacturer = _dbContext.Manufacturers
                .SingleOrDefault(d => d.Id == id);
            if (manufacturer == null) return BadRequest(new { message = string.Format("Manufacturer with id {0} doesn't exist", id) });
            return Ok(manufacturer);
        }

        [HttpPost("EditManufacturer")]
        public IActionResult EditManufacturer(ManufacturerEditModel manufacturerDetails)
        {
            var existingManufacturer = _dbContext.Manufacturers.FirstOrDefault(d => d.Id == manufacturerDetails.Id);

            if (existingManufacturer == null)
            {
                return NotFound(new { message = "Manufacturer not Found" });
            }

            if (_dbContext.Manufacturers.Any(d => d.Name.ToUpper() == manufacturerDetails.Name.ToUpper() && d.Id != manufacturerDetails.Id))
            {
                return BadRequest(new
                {
                    message = "Manufacturer with this Name already Exist"
                });
            }
            try
            {
                existingManufacturer.Name = manufacturerDetails.Name;
                _dbContext.SaveChanges();
                return Ok(existingManufacturer);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }
        [HttpPost("DeleteManufacturer")]
        public IActionResult DeleteManufacturer([FromBody] int id)
        {
            var existingManufacturer = _dbContext.Manufacturers.FirstOrDefault(d => d.Id == id);

            if (existingManufacturer == null)
            {
                return NotFound(new { message = "Manufacturer not Found" });
            }
            if (_dbContext.Equipments.Any(e => e.Manufacturer.Id == existingManufacturer.Id))
            {
                return BadRequest(new { message = $"{existingManufacturer.Name} is in use and it can't be delete." });
            }

            try
            {
                _dbContext.Manufacturers.Remove(existingManufacturer);
                _dbContext.SaveChanges();
                return Ok(new { message = $"{existingManufacturer.Name} has been deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }
        [HttpPost("AddManufacturers")]
        public IActionResult AddManufacturers([FromBody] List<string> names)
        {
            if (names == null || !names.Any())
            {
                return BadRequest(new { message = "No manufacturer names provided." });
            }

            var manufacturerNames = names.Select(name => name).ToList();

            var existingManufacturers = _dbContext.Manufacturers
                                                  .Where(m => manufacturerNames.Select(n => n.ToLower()).Contains(m.Name.ToLower()))
                                                  .Select(m => m.Name)
                                                  .ToList();

            if (existingManufacturers.Any())
            {
                return BadRequest(new { message = $"The following manufacturers already exist: {string.Join(", ", existingManufacturers)}" });
            }

            var newManufacturers = manufacturerNames.Select(name => new Manufacturer { Name = name }).ToList();

            _dbContext.Manufacturers.AddRange(newManufacturers);
            _dbContext.SaveChanges();

            return Ok(new { message = $"{newManufacturers.Count} manufacturers added successfully." });
        }
    }
}
