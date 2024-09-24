using LGMS.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        [HttpPost("AddEquipment")]
        public IActionResult AddEquipment(EquipmentAddModel equipmentDetails)
        {
            return Ok();
        }
    }
}
