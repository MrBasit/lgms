using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private LgmsDbContext _dbContext;
        private PagedData<Equipment> _pagedData;

        public EquipmentController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Equipment>();
        }

        [HttpPost("GetEquipments")]
        public IActionResult GetEquipments(EquipmentsSearchModel equipmentSearchModel)
        {
            if (equipmentSearchModel == null) return BadRequest("Invalid search criteria");

            var equipments = new List<Equipment>();

            try
            {
                equipments = _dbContext.Equipments
                    .Include(e => e.Type)
                    .Include(e => e.Assignees)
                    .Include(e => e.Manufacturer)
                    .Include(e => e.Vendor)
                    .Include(e => e.Status)
                    .ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (!equipments.Any()) return NotFound("Equipments Not Found");

            var equipmentWithSelectedStatuses = new List<Equipment>();
            foreach (var status in equipmentSearchModel.Statuses)
            {
                equipmentWithSelectedStatuses.AddRange(equipments.Where(x => x.Status.Id == status.Id).ToList());
            }

            equipments = equipmentWithSelectedStatuses;

            if (!string.IsNullOrEmpty(equipmentSearchModel.SearchDetails.SearchTerm))
            {
                var searchTerm = equipmentSearchModel.SearchDetails.SearchTerm.ToUpper();

                equipments = equipments.Where(e =>
                    e.Type.Title.ToUpper().Contains(searchTerm) ||
                    e.Manufacturer.Name.ToUpper().Contains(searchTerm) ||
                    e.Vendor.Name.ToUpper().Contains(searchTerm) ||
                    e.Assignees.Any(a => a.Name.ToUpper().Contains(searchTerm))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(equipmentSearchModel.SortDetails.SortColumn) &&
                equipmentSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (equipmentSearchModel.SortDetails.SortColumn)
                {
                    case "number":
                        equipments = equipmentSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Number).ToList()
                            : equipments.OrderByDescending(e => e.Number).ToList();
                        break;
                    case "manufacturer":
                        equipments = equipmentSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Manufacturer.Name).ToList()
                            : equipments.OrderByDescending(e => e.Manufacturer.Name).ToList();
                        break;
                    case "status":
                        equipments = equipmentSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Status.Title).ToList()
                            : equipments.OrderByDescending(e => e.Status.Title).ToList();
                        break;
                    case "vendor":
                        equipments = equipmentSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Vendor.Name).ToList()
                            : equipments.OrderByDescending(e => e.Vendor.Name).ToList();
                        break;
                    case "type":
                        equipments = equipmentSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Type.Title).ToList()
                            : equipments.OrderByDescending(e => e.Type.Title).ToList();
                        break;
                    default:
                        equipments = equipmentSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending
                            ? equipments.OrderBy(e => e.Number).ToList()
                            : equipments.OrderByDescending(e => e.Number).ToList();
                        break;
                }
            }
            else
            {
                equipments = equipments.OrderBy(e => e.Number).ToList();
            }


            var pagedEquipmentsResult = _pagedData.GetPagedData(
                equipments,
                (PagedDataRequestModel)equipmentSearchModel.PaginationDetails
            );

            return Ok(pagedEquipmentsResult);
        }


        [HttpPost("AddEquipment")]
        public IActionResult AddEquipment(EquipmentAddModel equipmentDetails)
        {
            var assignees = new List<Employee>();
            foreach (var assigneeDetails in equipmentDetails.Assignees)
            {

                var employee = _dbContext.Employees.SingleOrDefault(e => e.Id == assigneeDetails.Id);
                if (employee == null)
                {
                    return NotFound($"Employee with ID {assigneeDetails.Id} not found.");
                }

                assignees.Add(employee);
            }
            if (_dbContext.Equipments.Any(e => e.Number == equipmentDetails.Number))
            {
                return BadRequest("Another equipment with this number already exists");
            }

            try
            {
                Equipment equipment = new Equipment()
                {
                    Number = equipmentDetails.Number,
                    Type = equipmentDetails.Type.Id == 0
                        ? equipmentDetails.Type
                        : _dbContext.EquipmentTypes.Single(m => m.Id == equipmentDetails.Type.Id),
                    Manufacturer = equipmentDetails.Manufacturer.Id == 0
                        ? equipmentDetails.Manufacturer
                        : _dbContext.Manufacturers.Single(m => m.Id == equipmentDetails.Manufacturer.Id),
                    Assignees = assignees,
                    Status = equipmentDetails.Status.Id == 0
                        ? equipmentDetails.Status
                        : _dbContext.EquipmentStatus.Single(s => s.Id == equipmentDetails.Status.Id),
                    Comments = equipmentDetails.Comments,
                    Vendor = equipmentDetails.Vendor.Id == 0
                        ? equipmentDetails.Vendor
                        : _dbContext.Vendors.Single(m => m.Id == equipmentDetails.Vendor.Id),
                    WarrantyExpiryDate = equipmentDetails.WarrantyExpiryDate,
                    BuyingDate = equipmentDetails.BuyingDate,
                    UnboxingDate = equipmentDetails.UnboxingDate
                };
                _dbContext.Equipments.Add(equipment);
                _dbContext.SaveChanges();
                return Ok(equipment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("EditEquipment")]
        public IActionResult EditEquipment(EquipmentEditModel equipmentDetails)
        {
            var existingEquipment = _dbContext.Equipments
                .Include(e => e.Assignees)
                .FirstOrDefault(e => e.Id == equipmentDetails.Id);

            if (existingEquipment == null)
            {
                return NotFound("Equipment not found");
            }

            if (_dbContext.Equipments.Any(e => e.Number == equipmentDetails.Number && e.Id != equipmentDetails.Id))
            {
                return BadRequest("Another equipment with this number already exists");
            }

            var newAssigneeIds = new HashSet<int>(equipmentDetails.Assignees.Select(a => a.Id));
            var existingAssigneeIds = new HashSet<int>(existingEquipment.Assignees.Select(a => a.Id));

            foreach (var assignee in existingEquipment.Assignees.Where(a => !newAssigneeIds.Contains(a.Id)).ToList())
            {
                existingEquipment.Assignees.Remove(assignee);
            }

            foreach (var assigneeDetails in equipmentDetails.Assignees)
            {
                if (!existingAssigneeIds.Contains(assigneeDetails.Id))
                {
                    var employee = _dbContext.Employees.SingleOrDefault(e => e.Id == assigneeDetails.Id);
                    if (employee == null)
                    {
                        return NotFound($"Employee with ID {assigneeDetails.Id} not found.");
                    }

                    existingEquipment.Assignees.Add(employee);
                }
            }

            try
            {
                existingEquipment.Number = equipmentDetails.Number;
                existingEquipment.Type = equipmentDetails.Type.Id == 0
                    ? equipmentDetails.Type
                    : _dbContext.EquipmentTypes.Single(m => m.Id == equipmentDetails.Type.Id);
                existingEquipment.Manufacturer = equipmentDetails.Manufacturer.Id == 0
                    ? equipmentDetails.Manufacturer
                    : _dbContext.Manufacturers.Single(m => m.Id == equipmentDetails.Manufacturer.Id);
                existingEquipment.Status = equipmentDetails.Status.Id == 0
                    ? equipmentDetails.Status
                    : _dbContext.EquipmentStatus.Single(s => s.Id == equipmentDetails.Status.Id);
                existingEquipment.Comments = equipmentDetails.Comments;
                existingEquipment.Vendor = equipmentDetails.Vendor.Id == 0
                    ? equipmentDetails.Vendor
                    : _dbContext.Vendors.Single(m => m.Id == equipmentDetails.Vendor.Id);
                existingEquipment.WarrantyExpiryDate = equipmentDetails.WarrantyExpiryDate;
                existingEquipment.BuyingDate = equipmentDetails.BuyingDate;
                existingEquipment.UnboxingDate = equipmentDetails.UnboxingDate;
                _dbContext.SaveChanges();
                return Ok(existingEquipment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteEquipment")]
        public IActionResult DeleteEquipment([FromBody] int EquipmentId)
        {
            try
            {
                Equipment? equipment = _dbContext.Equipments.FirstOrDefault(e => e.Id == EquipmentId);
                EquipmentStatus? equipmentStatus =
                    _dbContext.EquipmentStatus.FirstOrDefault(s => s.Title.ToUpper() == "DELETED");
                if (equipment == null) return BadRequest("Equipment Id is not correct");
                if (equipmentStatus == null) return BadRequest("Deleted Status Not Found");

                equipment.Status = equipmentStatus;
                _dbContext.Equipments.Update(equipment);
                _dbContext.SaveChanges();

                return Ok(equipment);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetEquipmentTypesWithCount")]
        public ActionResult GetEquipmentTypesWithCount()
        {
            var equipmentTypesWithCount = _dbContext.EquipmentTypes
                .Select(t => new
                {
                    EquipmentName = t.Title,
                    EqipmentCount = _dbContext.Equipments.Where(e => e.Status.Title == "Active").Count(e => e.Type.Id == t.Id)
                })
                .OrderByDescending(t => t.EqipmentCount) 
                .Take(5) 
                .ToList()
                .Select(t => $"{t.EquipmentName} - ({t.EqipmentCount})");

            return Ok(equipmentTypesWithCount);
        }
        [HttpGet("GetAssigneesWithEquipmentCount")]
        public ActionResult GetAssigneesWithEquipmentCount()
        {
            var equipmentTypesWithCount = _dbContext.Employees
                .Include(e => e.Equipments)
                .Select(t => new
                {
                    AssigneeName = t.Name,
                    EqipmentCount = t.Equipments.Count()
                })
                .OrderByDescending(t => t.EqipmentCount)
                .Take(5)
                .ToList()
                .Select(t => $"{t.AssigneeName} - ({t.EqipmentCount})");

            return Ok(equipmentTypesWithCount);
        }
    }
}
