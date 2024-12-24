using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityDepositController : ControllerBase
    {
        public LgmsDbContext _dbContext;

        public SecurityDepositController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("AddSecurityDeposit")]
        public IActionResult AddSecurityDeposit(SecurityDepositAddModel securityDeposit)
        {
            var employee = _dbContext.Employees.SingleOrDefault(e => e.Id == securityDeposit.EmployeeId);
            if (employee == null) return BadRequest(new { message = $"Employee with Id {securityDeposit.EmployeeId} not found" });
            try
            {
                SecurityDeposit deposit = new SecurityDeposit()
                {
                    Employee = employee,
                    Amount = securityDeposit.Amount,
                    Date = securityDeposit.Date,
                    Title = securityDeposit.Title
                };
                _dbContext.SecurityDeposits.Add(deposit);
                _dbContext.SaveChanges();
                return Ok(deposit);
            }
            catch (Exception ex) 
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }
    }
}
