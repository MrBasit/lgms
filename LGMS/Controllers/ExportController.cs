using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly ExcelService _equipmentDownloadService;
        private LgmsDbContext _dbContext;

        public ExportController(ExcelService equipemntDownloadService, LgmsDbContext dbContext)
        {
            _equipmentDownloadService = equipemntDownloadService;
            _dbContext = dbContext;
        }

        [HttpPost("DownloadEquipment")]
        public IActionResult DownloadEquipment(EquipmentExportSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest("Invalid search criteria");
            var equipments = new List<Equipment>();

            try
            {
                equipments = _dbContext.Equipments
                    .Include(e => e.Status)
                    .Include(e => e.Manufacturer)
                    .Include(e => e.Vendor)
                    .Include(e => e.Type)
                    .Include(e => e.Assignees)
                    .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (!equipments.Any()) return NotFound("Equipments Not Found");

            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                var searchTerm = searchModel.SearchDetails.SearchTerm.ToUpper();
                equipments = equipments.Where(e =>
                    e.Type.Title.ToUpper().Contains(searchTerm) ||
                    e.Manufacturer.Name.ToUpper().Contains(searchTerm) ||
                    e.Vendor.Name.ToUpper().Contains(searchTerm) ||
                    e.Assignees.Any(a => a.Name.ToUpper().Contains(searchTerm))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "number":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Number).ToList()
                            : equipments.OrderByDescending(e => e.Number).ToList();
                        break;
                    case "manufacturer":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Manufacturer).ToList()
                            : equipments.OrderByDescending(e => e.Manufacturer).ToList();
                        break;
                    case "assignees":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Assignees).ToList()
                            : equipments.OrderByDescending(e => e.Assignees).ToList();
                        break;
                    case "status":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Status).ToList()
                            : equipments.OrderByDescending(e => e.Status).ToList();
                        break;
                    case "vendor":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Vendor).ToList()
                            : equipments.OrderByDescending(e => e.Vendor).ToList();
                        break;
                    case "type":
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Type).ToList()
                            : equipments.OrderByDescending(e => e.Type).ToList();
                        break;
                    default:
                        equipments = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Number).ToList()
                            : equipments.OrderByDescending(e => e.Number).ToList();
                        break;
                }
            }
            else
            {
                equipments = equipments.OrderBy(e => e.Number).ToList();
            }

            try
            {
                
                var fileContents = _equipmentDownloadService.GenerateEquipmentExcelFile(equipments);
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "equipment_data.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }


    }
}
