using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Authorize(Roles = "Stores")]
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentTypeController : ControllerBase
    {
        private LgmsDbContext _dbContext;
        private PagedData<EquipmentType> _pagedData;

        public EquipmentTypeController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<EquipmentType>();
        }

        [HttpGet("GetEquipmentTypes")]
        public IActionResult GetEquipmentTypes()
        {
            var equipmentTypes = _dbContext.EquipmentTypes.ToList();

            return Ok(equipmentTypes);
        }

        [HttpPost("GetEquipmentTypesWithFilters")]
        public IActionResult GetEquipmentTypesWithFilters(BaseSearchModel equipmentTypesSearchModel)
        {
            if (equipmentTypesSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });
            var equipmentTypes = new List<EquipmentType>();
            try
            {
                equipmentTypes = _dbContext.EquipmentTypes.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }

            if (!equipmentTypes.Any()) return NotFound(new { message = "No equipment types are there" });

            if (!string.IsNullOrEmpty(equipmentTypesSearchModel.SearchDetails.SearchTerm))
            {
                equipmentTypes = equipmentTypes.Where(v =>
                    v.Title.ToUpper().Contains(equipmentTypesSearchModel.SearchDetails.SearchTerm.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(equipmentTypesSearchModel.SortDetails.SortColumn) && equipmentTypesSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (equipmentTypesSearchModel.SortDetails.SortColumn)
                {
                    case "id":
                        equipmentTypes = equipmentTypesSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            equipmentTypes.OrderBy(e => e.Id).ToList() :
                            equipmentTypes.OrderByDescending(e => e.Id).ToList();
                        break;
                    case "title":
                        equipmentTypes = equipmentTypesSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            equipmentTypes.OrderBy(e => e.Title).ToList() :
                            equipmentTypes.OrderByDescending(e => e.Title).ToList();
                        break;
                    default:
                        equipmentTypes = equipmentTypesSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            equipmentTypes.OrderBy(e => e.Title).ToList() :
                            equipmentTypes.OrderByDescending(e => e.Title).ToList();
                        break;
                }
            }
            else
            {
                equipmentTypes = equipmentTypes.OrderBy(e => e.Id).ToList();
            }
            var pagedEquipmentTypesResult = _pagedData.GetPagedData(
                equipmentTypes,
                (PagedDataRequestModel)equipmentTypesSearchModel.PaginationDetails
            );

            return Ok(pagedEquipmentTypesResult);
        }
        [HttpGet("GetEquipmentTypeById")]
        public IActionResult GetEquipmentTypeById(int id)
        {
            var equipmentType = _dbContext.EquipmentTypes
                .SingleOrDefault(d => d.Id == id);
            if (equipmentType == null) return BadRequest(new { message = string.Format("Equipment type with id {0} doesn't exist", id) });
            return Ok(equipmentType);
        }

        [HttpPost("EditEquipmentType")]
        public IActionResult EditEquipmentType(EquipmentTypeEditModel equipmentTypeDetails)
        {
            var existingEquipmentType = _dbContext.EquipmentTypes.FirstOrDefault(d => d.Id == equipmentTypeDetails.Id);

            if (existingEquipmentType == null)
            {
                return NotFound(new { message = "Equipment Type not Found" });
            }

            if (_dbContext.EquipmentTypes.Any(d => d.Title.ToUpper() == equipmentTypeDetails.Title.ToUpper() && d.Id != equipmentTypeDetails.Id))
            {
                return BadRequest(new
                {
                    message = "Equipment Type with this Title already Exist"
                });
            }
            try
            {
                existingEquipmentType.Title = equipmentTypeDetails.Title;
                _dbContext.SaveChanges();
                return Ok(existingEquipmentType);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }
        [HttpPost("DeleteEquipmentType")]
        public IActionResult DeleteEquipmentType([FromBody] int id)
        {
            var existingEquipmentType = _dbContext.EquipmentTypes.FirstOrDefault(d => d.Id == id);

            if (existingEquipmentType == null)
            {
                return NotFound(new { message = "Equipment Type not Found" });
            }
            if (_dbContext.Equipments.Any(e => e.Type.Id == existingEquipmentType.Id))
            {
                return BadRequest(new { message = $"{existingEquipmentType.Title} is in use and it can't be delete." });
            }

            try
            {
                _dbContext.EquipmentTypes.Remove(existingEquipmentType);
                _dbContext.SaveChanges();
                return Ok(new { message = $"{existingEquipmentType.Title} has been deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }
        [HttpPost("AddEquipmentTypes")]
        public IActionResult AddEquipmentTypes([FromBody] List<string> titles)
        {
            if (titles == null || !titles.Any())
            {
                return BadRequest(new { message = "No equipment type titles provided." });
            }

            var typeNames = titles.Select(title => title).ToList();

            var existingEquipmentTypes = _dbContext.EquipmentTypes
                                            .Where(et => typeNames.Select(t => t.ToLower()).Contains(et.Title.ToLower()))
                                            .Select(et => et.Title)
                                            .ToList();

            if (existingEquipmentTypes.Any())
            {
                return BadRequest(new { message = $"The following equipment types already exist: {string.Join(", ", existingEquipmentTypes)}" });
            }

            var newEquipmentTypes = typeNames.Select(title => new EquipmentType { Title = title }).ToList();
            _dbContext.EquipmentTypes.AddRange(newEquipmentTypes);
            _dbContext.SaveChanges();

            return Ok(new { message = $"{newEquipmentTypes.Count} equipment types added successfully." });
        }

    }
}
