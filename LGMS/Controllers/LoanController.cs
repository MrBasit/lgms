using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        public LgmsDbContext _dbContext;

        public LoanController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("AddLoan")]
        public IActionResult AddLoan(LoanAddModel loan)
        {
            var employee = _dbContext.Employees.SingleOrDefault(e => e.Id == loan.EmployeeId);
            if (employee == null) return BadRequest(new { message = $"Employee with Id {loan.EmployeeId} not found" });
            try
            {
                Loan data = new Loan()
                {
                    Employee = employee,
                    Amount = loan.Amount,
                    Date = loan.Date,
                    Title = loan.Title
                };
                _dbContext.Loans.Add(data);
                _dbContext.SaveChanges();
                return Ok(data);
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
