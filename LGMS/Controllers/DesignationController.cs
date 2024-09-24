using LGMS.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignationController : ControllerBase
    {
        LgmsDbContext _dbContex;
        public DesignationController(LgmsDbContext dbContext)
        {
            _dbContex = dbContext;
        }

        [HttpGet("GetDesignations")]
        public IActionResult GetDesignations()
        {
            var designations = _dbContex.Designations.ToList();
            return Ok(designations);
        }
    }
}
