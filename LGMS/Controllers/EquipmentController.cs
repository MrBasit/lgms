using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using LGMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace LGMS.Controllers
{
    [Authorize(Roles = "Stores")]
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private LgmsDbContext _dbContext;
        private PagedData<Equipment> _pagedData;
        private readonly ImageService _imageService;
        public EquipmentController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Equipment>();
            _imageService = new ImageService();
        }

        [HttpPost("GetEquipmentsWithFilters")]
        public IActionResult GetEquipmentsWithFilters(EquipmentsSearchModel equipmentSearchModel)
        {
            if (equipmentSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });

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
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }

            //if (!equipments.Any()) return NotFound(new { message = "No equipments are there" });

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
                    (e.Number != null && e.Number.ToUpper().Contains(searchTerm)) ||
                    (e.Manufacturer?.Name?.ToUpper().Contains(searchTerm) ?? false) ||
                    (e.Vendor?.Name?.ToUpper().Contains(searchTerm) ?? false) ||
                    (e.Assignees?.Any(a => a.Name.ToUpper().Contains(searchTerm)) ?? false)
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

        [HttpGet("GetEquipments")]
        public IActionResult GetEquipments(int? excludeId = null)
        {
            var equipments = _dbContext.Equipments
                .Include(e => e.Type)
                .Include(e => e.Assignees)
                .Include(e => e.Manufacturer)
                .Include(e => e.Vendor)
                .Include(e => e.Status)
                .ToList();

            if (excludeId.HasValue)
            {
                equipments = equipments.Where(e => e.Id != excludeId.Value).ToList();
            }

            return Ok(equipments);
        }

        [HttpGet("GetEquipmentById")]
        public IActionResult GetEquipmentById(int equipmentId)
        {
            try
            {
                var equipment = _dbContext.Equipments
                    .Include(e => e.Type)
                    .Include(e => e.Assignees)
                    .Include(e => e.Status)
                    .Include(e => e.Manufacturer)
                    .Include(e => e.Vendor)
                    .Include(e => e.ParentEquipment)
                        .ThenInclude(eq => eq.Type)
                    .SingleOrDefault(e => e.Id == equipmentId);

                if (equipment == null)
                    return NotFound(new { message = $"Equipment with ID {equipmentId} does not exist." });

                var childEquipments = _dbContext.Equipments
                    .Where(e => e.ParentEquipment != null && e.ParentEquipment.Id == equipmentId).Include(e => e.Type)
                    .ToList();

                var result = new
                {
                    Equipment = equipment,
                    ChildEquipments = childEquipments
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "") });
            }
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
                    return NotFound(new { message = $"Employee with ID {assigneeDetails.Id} not found." });
                }

                assignees.Add(employee);
            }
            if (_dbContext.Equipments.Any(e => e.Number == equipmentDetails.Number))
            {
                return BadRequest(new { message = "Another equipment with this number already exists" });
            }
            string parentEquipmentNumber = null;
            if (!string.IsNullOrEmpty(equipmentDetails.ParentEquipmentNumber))
            {
                parentEquipmentNumber = equipmentDetails.ParentEquipmentNumber.Split(new[] { " - " }, StringSplitOptions.None)[0];
            }
            var parentEquipment =
                _dbContext.Equipments.SingleOrDefault(e => e.Number == parentEquipmentNumber);

            string number;
            if (string.IsNullOrEmpty(equipmentDetails.Number) || equipmentDetails.Number == null || (equipmentDetails.Number.StartsWith("EQ") && equipmentDetails.Number.Substring(2).All(char.IsDigit)))
            {
                number = GenerateEquipmentNumber();
            }else
            {
                number = equipmentDetails.Number;
            }

            try
            {
                Equipment equipment = new Equipment()
                {
                    Number = number,
                    Type = equipmentDetails.Type.Id == 0
                        ? equipmentDetails.Type
                        : _dbContext.EquipmentTypes.Single(m => m.Id == equipmentDetails.Type.Id),
                    Manufacturer = equipmentDetails.Manufacturer != null? equipmentDetails.Manufacturer.Id == 0
                        ? equipmentDetails.Manufacturer
                        : _dbContext.Manufacturers.Single(m => m.Id == equipmentDetails.Manufacturer.Id):null,
                    Assignees = assignees,
                    Status = equipmentDetails.Status.Id == 0
                        ? equipmentDetails.Status
                        : _dbContext.EquipmentStatus.Single(s => s.Id == equipmentDetails.Status.Id),
                    Description = equipmentDetails.Description,
                    Vendor = equipmentDetails.Vendor != null? equipmentDetails.Vendor.Id == 0
                        ? equipmentDetails.Vendor
                        : _dbContext.Vendors.Single(m => m.Id == equipmentDetails.Vendor.Id):null,
                    WarrantyExpiryDate = equipmentDetails.WarrantyExpiryDate,
                    BuyingDate = equipmentDetails.BuyingDate,
                    UnboxingDate = equipmentDetails.UnboxingDate,
                    ParentEquipment = parentEquipment
                };
                _dbContext.Equipments.Add(equipment);
                _dbContext.SaveChanges();
                return Ok(equipment);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [HttpPost("EditEquipment")]
        public IActionResult EditEquipment(EquipmentEditModel equipmentDetails)
        {
            var existingEquipment = _dbContext.Equipments
                .Include(e => e.Manufacturer)
                .Include(e => e.Vendor)
                .Include(e => e.ParentEquipment)
                .ThenInclude(eq => eq.Type)
                .Include(e => e.Assignees)
                .FirstOrDefault(e => e.Id == equipmentDetails.Id);

            if (existingEquipment == null)
            {
                return NotFound(new { message = "Equipment not found" });
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
                        return NotFound(new { message = $"Employee with ID {assigneeDetails.Id} not found." });
                    }

                    existingEquipment.Assignees.Add(employee);
                }
            }
            string parentEquipmentNumber = null;
            if (!string.IsNullOrEmpty(equipmentDetails.ParentEquipmentNumber))
            {
                parentEquipmentNumber = equipmentDetails.ParentEquipmentNumber.Split(new[] { " - " }, StringSplitOptions.None)[0];
            }

            var parentEquipment =
                _dbContext.Equipments.Include(e => e.ParentEquipment).ThenInclude(eq => eq.Type).SingleOrDefault(e => e.Number == parentEquipmentNumber);
            if (parentEquipment != null && parentEquipment == existingEquipment)
            {
                return BadRequest(new { message = "Cannot make equipment its own parent" });
            }
            if (parentEquipment != null && parentEquipment.ParentEquipment == existingEquipment)
            {
                return BadRequest(new { message = "Cannot assign parent equipment as it is a child of the equipment being edited." });
            }

            try
            {
                existingEquipment.Type = equipmentDetails.Type.Id == 0
                    ? equipmentDetails.Type
                    : _dbContext.EquipmentTypes.Single(m => m.Id == equipmentDetails.Type.Id);
                existingEquipment.Manufacturer = equipmentDetails.Manufacturer != null ? equipmentDetails.Manufacturer.Id == 0
                        ? equipmentDetails.Manufacturer
                        : _dbContext.Manufacturers.Single(m => m.Id == equipmentDetails.Manufacturer.Id) : null;
                existingEquipment.Status = equipmentDetails.Status.Id == 0
                    ? equipmentDetails.Status
                    : _dbContext.EquipmentStatus.Single(s => s.Id == equipmentDetails.Status.Id);
                existingEquipment.Description = equipmentDetails.Description;
                existingEquipment.Vendor = equipmentDetails.Vendor != null ? equipmentDetails.Vendor.Id == 0
                        ? equipmentDetails.Vendor
                        : _dbContext.Vendors.Single(m => m.Id == equipmentDetails.Vendor.Id) : null;
                existingEquipment.WarrantyExpiryDate = equipmentDetails.WarrantyExpiryDate;
                existingEquipment.BuyingDate = equipmentDetails.BuyingDate;
                existingEquipment.UnboxingDate = equipmentDetails.UnboxingDate;
                existingEquipment.ParentEquipment = parentEquipment;
                _dbContext.SaveChanges();
                return Ok(existingEquipment);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
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
                if (equipment == null) return BadRequest(new { message = "Equipment Id is not correct" });
                if (equipmentStatus == null) return BadRequest(new { message = "Deleted Status Not Found" });

                equipment.Status = equipmentStatus;
                _dbContext.Equipments.Update(equipment);
                _dbContext.SaveChanges();

                return Ok(equipment);

            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }
        
        [HttpGet("GetEquipmentTypesWithCount")]
        public ActionResult GetEquipmentTypesWithCount()
        {
            var equipmentTypesWithCount = _dbContext.EquipmentTypes
                .Select(t => new
                {
                    EquipmentName = t.Title,
                    EqipmentCount = _dbContext.Equipments.Where(e => e.Status.Title != "Discard" && e.Status.Title != "Deleted").Count(e => e.Type.Id == t.Id)
                })
                .OrderByDescending(t => t.EqipmentCount) 
                .Select(t => $"{t.EquipmentName} - {t.EqipmentCount}");

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
                    EqipmentCount = t.Equipments.Count(e => e.Status.Title != "Discard" && e.Status.Title != "Deleted")
                })
                .OrderByDescending(t => t.EqipmentCount)
                .Select(t => $"{t.AssigneeName} - {t.EqipmentCount}");

            return Ok(equipmentTypesWithCount);
        }
        
        [HttpGet("GetStatusesWithEquipmentCount")]
        public ActionResult GetStatusesWithEquipmentCount()
        {
            var equipmentTypesWithCount = _dbContext.EquipmentStatus
                .GroupJoin(
                    _dbContext.Equipments,  
                    status => status.Id,    
                    equipment => equipment.Status.Id, 
                    (status, equipmentGroup) => new
                    {
                        StatusName = status.Title,
                        EquipmentCount = equipmentGroup.Count() 
                    }
                )
                .OrderByDescending(t => t.EquipmentCount)  
                .Select(t => $"{t.StatusName} - {t.EquipmentCount}")  
                .ToList();

            return Ok(equipmentTypesWithCount);
        }

        [HttpPost("generate-image")]
        public IActionResult GenerateImage([FromBody] List<int> equipmentIds) 
        {
            var equipments = _dbContext.Equipments
                .Include(e => e.Type)
                .Include(e => e.Assignees)
                    .ThenInclude(a => a.Status)
                .Include(e => e.Assignees)
                    .ThenInclude(a => a.Department)
                .Include(e => e.Assignees)
                    .ThenInclude(a => a.Designation)
                .Where(e => equipmentIds.Contains(e.Id))
                .ToList();

            var data = equipments.Select(equipment => (
                Code: $"{equipment.Number}\n{equipment.Type.Title}",
                Description: GetFormattedAssignees(equipment.Assignees)
            )).ToList();

            var imageBytes = _imageService.GenerateImage(data);
            return File(imageBytes, "image/png", "generated-image.png");
        }

        private string GetFormattedAssignees(IEnumerable<Employee> assignees)
        {
            if (assignees == null || !assignees.Any())
                return "-";

            var assigneeNames = assignees.Select(a => a.Name).Take(3).ToList();
            var formattedDescription = string.Join(Environment.NewLine, assigneeNames.Take(2)); 

            if (assigneeNames.Count > 2)
            {
                formattedDescription += " .....";
            }

            return formattedDescription;
        }

        private string GenerateEquipmentNumber()
        {
            var lastEquipment = _dbContext.Equipments
                .Where(c => c.Number.StartsWith("EQ"))
                .OrderByDescending(c => c.Number)
                .ToList()
                .FirstOrDefault(c => c.Number.Substring(2).All(char.IsDigit));


            if (lastEquipment == null)
            {
                return "EQ0001";
            }
            

            var lastEquipmentNumber = lastEquipment.Number;
            var numberPart = lastEquipmentNumber.Substring(2);
            var nextNumber = (int.Parse(numberPart) + 1).ToString();
            return "EQ" + nextNumber.PadLeft(Math.Max(numberPart.Length, nextNumber.Length), '0');
        }

    }
}
