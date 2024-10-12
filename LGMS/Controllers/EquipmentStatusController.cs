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
        [HttpGet("GetEquipmentStatusById")]
        public IActionResult GetEquipmentStatusById(int id)
        {
            var equipmentStatus = _dbContext.EquipmentStatus
                .SingleOrDefault(d => d.Id == id);
            if (equipmentStatus == null) return BadRequest(new { message = string.Format("EquipmentStatus with id {0} doesn't exist", id) });
            return Ok(equipmentStatus);
        }

        [HttpPost("EditEquipmentStatus")]
        public IActionResult EditEquipmentStatus(EquipmentStatusEditModel statusDetails)
        {
            var existingStatus = _dbContext.EquipmentStatus.FirstOrDefault(d => d.Id == statusDetails.Id);

            if (existingStatus == null)
            {
                return NotFound("EquipmentStatus not Found");
            }

            if (_dbContext.EquipmentStatus.Any(d => d.Title.ToUpper() == statusDetails.Title.ToUpper() && d.Id != statusDetails.Id))
            {
                return BadRequest(new
                {
                    message = "EquipmentStatus with this Title already Exist"
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
                    message = ex.Message,
                    innerMessage = ex.InnerException != null ? ex.InnerException.Message : ""
                });
            }
        }
        [HttpPost("DeleteEquipmentStatus")]
        public IActionResult DeleteEquipmentStatus([FromBody] int id)
        {
            var existingStatus = _dbContext.EquipmentStatus.FirstOrDefault(d => d.Id == id);

            if (existingStatus == null)
            {
                return NotFound("EquipmentStatus not Found");
            }

            try
            {
                _dbContext.EquipmentStatus.Remove(existingStatus);
                _dbContext.SaveChanges();
                return Ok(new { message = $"{existingStatus.Title} has been deleted." });
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
