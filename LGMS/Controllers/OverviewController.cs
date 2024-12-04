using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using LGMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LGMS.Controllers
{
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
            var currentMonth = DateTime.Now.Month;
            var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var currentYear = DateTime.Now.Year;
            var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var currentMonthContractsCount = _dbContext.Contracts
                .Where(c => c.StartDate.Month == currentMonth && c.StartDate.Year == currentYear)
                .Count();

            var lastMonthContractsCount = _dbContext.Contracts
                .Where(c => c.StartDate.Month == lastMonth && c.StartDate.Year == lastMonthYear)
                .Count();

            double percentageChange = 0;
            if (lastMonthContractsCount > 0)
            {
                percentageChange = ((double)(currentMonthContractsCount - lastMonthContractsCount) / lastMonthContractsCount) * 100;
            }
            else if (currentMonthContractsCount > 0)
            {
                percentageChange = 100;
            }

            return Ok(new
            {
                CurrentMonthContracts = currentMonthContractsCount,
                PercentageChange = percentageChange
            });
        }

        [HttpGet("GetRetainContracts")]
        public IActionResult GetRetainContracts()
        {
            var currentMonth = DateTime.Now.Month;
            var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var currentYear = DateTime.Now.Year;
            var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var previousClients = _dbContext.Contracts
                .Where(c => c.StartDate < new DateTime(currentYear, currentMonth, 1))
                .Select(c => c.Client.Id)
                .Distinct()
                .ToList();

            var currentMonthRetainedContractsCount = _dbContext.Contracts
                .Where(c => c.StartDate.Month == currentMonth && c.StartDate.Year == currentYear && previousClients.Contains(c.Client.Id))
                .Count();

            var lastMonthRetainedContractsCount = _dbContext.Contracts
                .Where(c => c.StartDate.Month == lastMonth && c.StartDate.Year == lastMonthYear && previousClients.Contains(c.Client.Id))
                .Count();

            double percentageChange = 0;
            if (lastMonthRetainedContractsCount > 0)
            {
                percentageChange = ((double)(currentMonthRetainedContractsCount - lastMonthRetainedContractsCount) / lastMonthRetainedContractsCount) * 100;
            }
            else if (currentMonthRetainedContractsCount > 0)
            {
                percentageChange = 100;
            }

            return Ok(new
            {
                RetainedContractsCount = currentMonthRetainedContractsCount,
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
                CurrentMonthContracts = currentMonthContractsCount
            });
        }

        [HttpGet("GetFulfilledContracts")]
        public IActionResult GetFulfilledContracts()
        {
            var currentMonth = DateTime.Now.Month;
            var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var currentYear = DateTime.Now.Year;
            var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var currentMonthContractsCount = _dbContext.Contracts
               .Where(c => c.CompletionDate != null &&
                    c.CompletionDate.Value.Month == currentMonth &&
                    c.CompletionDate.Value.Year == currentYear && c.Status.Title == "Completed")
               .Count();

            var lastMonthContractsCount = _dbContext.Contracts
                .Where(c => c.CompletionDate != null &&
                        c.CompletionDate.Value.Month == lastMonth &&
                        c.CompletionDate.Value.Year == lastMonthYear && c.Status.Title == "Completed")
                   .Count();

            double percentageChange = 0;
            if (lastMonthContractsCount > 0)
            {
                percentageChange = ((double)(currentMonthContractsCount - lastMonthContractsCount) / lastMonthContractsCount) * 100;
            }
            else if (currentMonthContractsCount > 0)
            {
                percentageChange = 100;
            }

            return Ok(new
            {
                CurrentMonthContracts = currentMonthContractsCount,
                PercentageChange = percentageChange
            });
        }

        [HttpGet("GetPayments")]
        public IActionResult GetPayments()
        {
            var currentMonth = DateTime.Now.Month;
            var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var currentYear = DateTime.Now.Year;
            var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var currentMonthPayments = _dbContext.Payments
               .Where(c => c.Date.Month == currentMonth &&
                    c.Date.Year == currentYear)
               .Sum(c => c.Amount);

            var lastMonthPayments = _dbContext.Payments
                .Where(c => c.Date.Month == lastMonth &&
                        c.Date.Year == lastMonthYear)
                .Sum(c => c.Amount);

            double percentageChange = 0;
            if (lastMonthPayments > 0)
            {
                percentageChange = ((double)(currentMonthPayments - lastMonthPayments) / lastMonthPayments) * 100;
            }
            else if (currentMonthPayments > 0)
            {
                percentageChange = 100;
            }

            return Ok(new
            {
                CurrentMonthContracts = currentMonthPayments,
                PercentageChange = percentageChange
            });
        }



    }
}
