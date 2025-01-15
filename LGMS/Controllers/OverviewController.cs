using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using LGMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LGMS.Controllers
{
    [Authorize(Roles = "Sales")]
    [Route("api/[controller]")]
    [ApiController]
    public class OverviewController : ControllerBase
    {
        private readonly LgmsDbContext _dbContext;
        private readonly OverviewService _overviewService;

        public OverviewController(LgmsDbContext dbContext, OverviewService overviewService)
        {
            _dbContext = dbContext;
            _overviewService = overviewService;
        }

        [HttpGet("GetContractsByRange")]
        public IActionResult GetContractsByRange(string range)
        {
            DateTime startDate;

            switch (range)
            {
                case "1 Month":
                    startDate = DateTime.Now.AddMonths(-1);
                    break;
                case "3 Months":
                    startDate = DateTime.Now.AddMonths(-3);
                    break;
                case "6 Months":
                    startDate = DateTime.Now.AddMonths(-6);
                    break;
                case "1 Year":
                    startDate = DateTime.Now.AddYears(-1);
                    break;
                case "2 Years":
                    startDate = DateTime.Now.AddYears(-2);
                    break;
                default:
                    return BadRequest("Invalid range");
            }

            var contracts = _dbContext.Contracts
                .Include(c => c.Type)
                .Where(c => c.CompletionDate.HasValue && c.CompletionDate >= startDate && c.Status.Title == "Completed")
                .ToList();

            var response = new
            {
                labels = _overviewService.GetTimeLabels(range),
                data = _overviewService.GetContractCountsByTimePeriod(contracts, range)
            };

            return Ok(response);
        }

        [HttpGet("GetNewContracts")]
        public IActionResult GetNewContracts()
        {
            var today = DateTime.Today;
            var startDateForCurrentPeriod = today.AddDays(-30);
            var startDateForPreviousPeriod = today.AddDays(-60);

            var previousClients = _dbContext.Contracts
                .Where(c => c.StartDate < startDateForCurrentPeriod)
                .Select(c => c.Client.Id)
                .Distinct()
                .ToList();

            var currentPeriodNewContractsCount = _dbContext.Contracts
                .Where(c => c.StartDate >= startDateForCurrentPeriod
                            && c.StartDate <= today
                            && !previousClients.Contains(c.Client.Id))
                .Count();

            var previousPeriodNewContractsCount = _dbContext.Contracts
                .Where(c => c.StartDate >= startDateForPreviousPeriod
                            && c.StartDate < startDateForCurrentPeriod
                            && !previousClients.Contains(c.Client.Id))
                .Count();

            int percentageChange = 0;
            if (previousPeriodNewContractsCount > 0)
            {
                percentageChange = (int)Math.Round(((double)(currentPeriodNewContractsCount - previousPeriodNewContractsCount) / previousPeriodNewContractsCount) * 100);
            }
            else if (currentPeriodNewContractsCount > 0)
            {
                percentageChange = 100;
            }

            return Ok(new
            {
                NewContractsCount = currentPeriodNewContractsCount,
                PercentageChange = percentageChange
            });
        }


        [HttpGet("GetRetainContracts")]
        public IActionResult GetRetainContracts()
        {
            var today = DateTime.Today;
            var startDateForCurrentPeriod = today.AddDays(-30);
            var startDateForPreviousPeriod = today.AddDays(-60);

            var previousClients = _dbContext.Contracts
                .Where(c => c.StartDate < startDateForCurrentPeriod)
                .Select(c => c.Client.Id)
                .Distinct()
                .ToList();

            var currentPeriodRetainedContractsCount = _dbContext.Contracts
                .Where(c => c.StartDate >= startDateForCurrentPeriod && c.StartDate <= today && previousClients.Contains(c.Client.Id))
                .Count();

            var previousPeriodRetainedContractsCount = _dbContext.Contracts
                .Where(c => c.StartDate >= startDateForPreviousPeriod && c.StartDate < startDateForCurrentPeriod && previousClients.Contains(c.Client.Id))
                .Count();

            int percentageChange = 0;
            if (previousPeriodRetainedContractsCount > 0)
            {
                percentageChange = (int)Math.Round(((double)(currentPeriodRetainedContractsCount - previousPeriodRetainedContractsCount) / previousPeriodRetainedContractsCount) * 100);
            }
            else if (currentPeriodRetainedContractsCount > 0)
            {
                percentageChange = 100;
            }

            return Ok(new
            {
                RetainContractsCount = currentPeriodRetainedContractsCount,
                PercentageChange = percentageChange
            });
        }

        [HttpGet("GetActiveContracts")]
        public IActionResult GetActiveContracts()
        {
            var currentMonthContractsCount = _dbContext.Contracts
                .Where(c => c.Status.Title == "Active")
                .Count();

            return Ok(new
            {
                ActiveContractsCount = currentMonthContractsCount
            });
        }

        [HttpGet("GetFulfilledContracts")]
        public IActionResult GetFulfilledContracts()
        {
            var today = DateTime.Today;
            var startDateForCurrentPeriod = today.AddDays(-30);
            var startDateForPreviousPeriod = today.AddDays(-60);

            var currentPeriodContractsCount = _dbContext.Contracts
                .Where(c => c.CompletionDate != null &&
                            c.CompletionDate.Value >= startDateForCurrentPeriod &&
                            c.CompletionDate.Value <= today &&
                            c.Status.Title == "Completed")
                .Count();

            var previousPeriodContractsCount = _dbContext.Contracts
                .Where(c => c.CompletionDate != null &&
                            c.CompletionDate.Value >= startDateForPreviousPeriod &&
                            c.CompletionDate.Value < startDateForCurrentPeriod &&
                            c.Status.Title == "Completed")
                .Count();

            int percentageChange = 0;
            if (previousPeriodContractsCount > 0)
            {
                percentageChange = (int)Math.Round(((double)(currentPeriodContractsCount - previousPeriodContractsCount) / previousPeriodContractsCount) * 100);
            }
            else if (currentPeriodContractsCount > 0)
            {
                percentageChange = 100;
            }

            return Ok(new
            {
                CompletedContractsCount = currentPeriodContractsCount,
                PercentageChange = percentageChange
            });
        }


        [HttpGet("GetPayments")]
        public IActionResult GetPayments()
        {
            var today = DateTime.Today;
            var startDateForCurrentPeriod = today.AddDays(-30);
            var startDateForPreviousPeriod = today.AddDays(-60);

            var currentPeriodPayments = _dbContext.Payments
                .Where(c => c.Date >= startDateForCurrentPeriod && c.Date <= today)
                .Sum(c => c.Amount);

            var previousPeriodPayments = _dbContext.Payments
                .Where(c => c.Date >= startDateForPreviousPeriod && c.Date <= startDateForCurrentPeriod)
                .Sum(c => c.Amount);

            int percentageChange = 0;
            if (previousPeriodPayments > 0)
            {
                percentageChange = (int)Math.Round(((double)(currentPeriodPayments - previousPeriodPayments) / previousPeriodPayments) * 100);
            }
            else if (currentPeriodPayments > 0)
            {
                percentageChange = 100;
            }

            return Ok(new
            {
                PaymentsCount = currentPeriodPayments,
                PercentageChange = percentageChange
            });
        }

        [HttpGet("GetWeeklyExpirations")]
        public IActionResult GetWeeklyExpirations()
        {
            var today = DateTime.Today;
            var sevenDaysLater = today.AddDays(7);
            var expirations = _dbContext.Expiration.
                OrderBy(e => e.Date);
            var expirationCount = _dbContext.Expiration
                    .Where(e => e.Date >= today && e.Date <= sevenDaysLater)
                    .Count();

            return Ok(new { Count = expirationCount });

        }

    }
}
