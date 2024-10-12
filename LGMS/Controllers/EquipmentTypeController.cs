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
        public IActionResult GetEquipmentTypesWithFilters(EquipmentTypesSearchModel equipmentTypesSearchModel)
        {
            if (equipmentTypesSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });
            var equipmentTypes = new List<EquipmentType>();
            try
            {
                equipmentTypes = _dbContext.EquipmentTypes.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            if (!equipmentTypes.Any()) return NotFound(new { message = "EquipmentType Not Found" });

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
                return NotFound("Equipment Type not Found");
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
                    message = ex.Message,
                    innerMessage = ex.InnerException != null ? ex.InnerException.Message : ""
                });
            }
        }
        [HttpPost("DeleteEquipmentType")]
        public IActionResult DeleteEquipmentType([FromBody] int id)
        {
            var existingEquipmentType = _dbContext.EquipmentTypes.FirstOrDefault(d => d.Id == id);

            if (existingEquipmentType == null)
            {
                return NotFound("Equipment Type not Found");
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
                    message = ex.Message,
                    innerMessage = ex.InnerException != null ? ex.InnerException.Message : ""
                });
            }
        }
    }
}
