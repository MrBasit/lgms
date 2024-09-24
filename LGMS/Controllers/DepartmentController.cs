using LGMS.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        LgmsDbContext _dbContex;
        public DepartmentController(LgmsDbContext dbContext)
        {
            _dbContex = dbContext;
        }

        [HttpGet("GetDepartments")]
        public IActionResult GetDepartments()
        {
            var departments = _dbContex.Departments.ToList();
            return Ok(departments);
        }
    }
}
