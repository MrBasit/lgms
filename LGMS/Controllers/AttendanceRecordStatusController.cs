using LGMS.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceRecordStatusController : ControllerBase
    {
        private LgmsDbContext _context;

        public AttendanceRecordStatusController( LgmsDbContext dbContext)
        {
                _context = dbContext;
        }

        [HttpGet("GetAttendanceRecordStatuses")]
        public IActionResult GetAttendanceRecordStatuses()
        {
            var statuses = _context.AttendanceRecordStatuses
                                   .Where(s => s.Title == "On Leave" || s.Title == "Holiday" || s.Title == "Weekend" || s.Title == "Day Off")
                                   .ToList();
            return Ok(statuses);
        }
    }
}
