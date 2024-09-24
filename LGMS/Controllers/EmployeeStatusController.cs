using LGMS.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeStatusController : ControllerBase
    {
        LgmsDbContext _dbContex;
        public EmployeeStatusController(LgmsDbContext dbContext)
        {
            _dbContex = dbContext;
        }

        [HttpGet("GetEmployeeStatuses")]
        public IActionResult GetEmployeeStatuses()
        {
            var employeeStatuses = _dbContex.EmployeeStatus.ToList();
            return Ok(employeeStatuses);
        }
    }
}
