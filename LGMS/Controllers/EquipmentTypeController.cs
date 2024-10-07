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
    }
}
