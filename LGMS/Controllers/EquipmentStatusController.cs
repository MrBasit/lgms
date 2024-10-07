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
    public class EquipmentStatusController : ControllerBase
    {
        private LgmsDbContext _dbContext;
        private PagedData<EquipmentStatus> _pagedData;

        public EquipmentStatusController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<EquipmentStatus>();
        }

        [HttpGet("GetEquipmentStatuses")]
        public IActionResult GetEquipmentStatus()
        {
            var equipmentStatuses = _dbContext.EquipmentStatus.ToList();

            return Ok(equipmentStatuses);
        }

        [HttpPost("GetEquipmentStatusesWithFilters")]
        public IActionResult GetEquipmentStatusesWithFilters(EquipmentStatusesSearchModel equipmentStatusesSearchModel)
        {
            if (equipmentStatusesSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });
            var equipmentStatuses = new List<EquipmentStatus>();
            try
            {
                equipmentStatuses = _dbContext.EquipmentStatus.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            if (!equipmentStatuses.Any()) return NotFound(new { message = "EquipmentStatus Not Found" });

            if (!string.IsNullOrEmpty(equipmentStatusesSearchModel.SearchDetails.SearchTerm))
            {
                equipmentStatuses = equipmentStatuses.Where(v =>
                    v.Title.ToUpper().Contains(equipmentStatusesSearchModel.SearchDetails.SearchTerm.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(equipmentStatusesSearchModel.SortDetails.SortColumn) && equipmentStatusesSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (equipmentStatusesSearchModel.SortDetails.SortColumn)
                {
                    case "id":
                        equipmentStatuses = equipmentStatusesSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            equipmentStatuses.OrderBy(e => e.Id).ToList() :
                            equipmentStatuses.OrderByDescending(e => e.Id).ToList();
                        break;
                    case "title":
                        equipmentStatuses = equipmentStatusesSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            equipmentStatuses.OrderBy(e => e.Title).ToList() :
                            equipmentStatuses.OrderByDescending(e => e.Title).ToList();
                        break;
                    default:
                        equipmentStatuses = equipmentStatusesSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            equipmentStatuses.OrderBy(e => e.Title).ToList() :
                            equipmentStatuses.OrderByDescending(e => e.Title).ToList();
                        break;
                }
            }
            else
            {
                equipmentStatuses = equipmentStatuses.OrderBy(e => e.Id).ToList();
            }
            var pagedEquipmentStatusesResult = _pagedData.GetPagedData(
                equipmentStatuses,
                (PagedDataRequestModel)equipmentStatusesSearchModel.PaginationDetails
            );

            return Ok(pagedEquipmentStatusesResult);
        }
    }
}
