using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<BankAccount> _pagedData;
        public BankAccountController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<BankAccount>();
        }

        [HttpGet("GetBankAccounts")]
        public IActionResult GetBankAccounts()
        {
            var accounts = _dbContext.BankAccounts.ToList();
            return Ok(accounts);
        }

        [HttpPost("GetBankAccountsWithFilters")]
        public IActionResult GetBankAccountsWithFilters(BaseSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var bankAccounts = new List<BankAccount>();

            try
            {
                bankAccounts = _dbContext.BankAccounts.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!bankAccounts.Any()) return NotFound(new { message = "No bank accounts are there" });


            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                bankAccounts = bankAccounts.Where(e =>
                    e.AccountTitle.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.BankName.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper())
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "title":
                        bankAccounts = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    bankAccounts.OrderBy(e => e.AccountTitle).ToList() :
                                    bankAccounts.OrderByDescending(e => e.AccountTitle).ToList();
                        break;
                    case "bankName":
                        bankAccounts = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    bankAccounts.OrderBy(e => e.BankName).ToList() :
                                    bankAccounts.OrderByDescending(e => e.BankName).ToList();
                        break;
                    default:
                        bankAccounts = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    bankAccounts.OrderBy(e => e.AccountTitle).ToList() :
                                    bankAccounts.OrderByDescending(e => e.AccountTitle).ToList();
                        break;
                }
            }
            else
            {
                bankAccounts = bankAccounts.OrderBy(e => e.AccountTitle).ToList();
            }

            var pagedAccountsResult = _pagedData.GetPagedData(
                bankAccounts,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedAccountsResult);
        }

        [HttpGet("GetBankAccountById")]
        public IActionResult GetBankAccountById(int id)
        {
            var bankAccount = _dbContext.BankAccounts.SingleOrDefault(e => e.Id == id);
            if (bankAccount == null) return BadRequest(new { message = string.Format("Bank Account with id {0} doesn't exist", id) });
            return Ok(bankAccount);

        }

        [HttpPost("AddBankAccount")]
        public IActionResult AddBankAccount(BankAccountAddModel details)
        {
            if (_dbContext.BankAccounts.Any(e => e.IBAN.ToUpper() == details.IBAN.ToUpper()))
            {
                return BadRequest(new { message = "Another bank account with this IBAN number already exists" });
            }
            if (_dbContext.BankAccounts.Any(e => e.BankName.ToUpper() == details.BankName.ToUpper() && e.AccountNumber == details.AccountNumber))
            {
                return BadRequest(new { message = "Another bank account with this bank name and account number already exists" });
            }
            try
            {
                BankAccount bankAccount = new BankAccount()
                {
                    AccountTitle = details.AccountTitle,
                    BankName = details.BankName,
                    AccountNumber = details.AccountNumber,
                    IBAN = details.IBAN
                };

                _dbContext.BankAccounts.Add(bankAccount);
                _dbContext.SaveChanges();
                return Ok(bankAccount);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    innerMessage = ex.InnerException != null ? ex.InnerException.Message : ""
                });
            }
        }

        [HttpPost("EditBankAccount")]
        public ActionResult EditBankAccount(BankAccountEditModel details)
        {
            var existingAccount = _dbContext.BankAccounts
                .FirstOrDefault(e => e.Id == details.Id);

            if (existingAccount == null)
            {
                return NotFound(new { message = "Bank account not found" });
            }
            if (_dbContext.BankAccounts.Any(e => e.IBAN.ToUpper() == details.IBAN.ToUpper() && e.Id != details.Id))
            {
                return BadRequest(new { message = "Another bank account with this IBAN number already exists" });
            }
            if (_dbContext.BankAccounts.Any(e => e.BankName.ToUpper() == details.BankName.ToUpper() && e.AccountNumber == details.AccountNumber && e.Id != details.Id))
            {
                return BadRequest(new { message = "Another bank account with this bank name and account number already exists" });
            }

            try
            {
                existingAccount.AccountTitle = details.AccountTitle;
                existingAccount.BankName = details.BankName;
                existingAccount.AccountNumber = details.AccountNumber;
                existingAccount.IBAN = details.IBAN;

                _dbContext.SaveChanges();

                return Ok(existingAccount);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    innerMessage = ex.InnerException != null ? ex.InnerException.Message : ""
                });
            }
        }
        [HttpPost("DeleteBankAccount")]
        public IActionResult DeleteBankAccount([FromBody] int id)
        {
            var account = _dbContext.BankAccounts.FirstOrDefault(a => a.Id == id);
            if(_dbContext.Quotations.Any(q => q.BankAccounts.Any(b => b.Id == id)) || _dbContext.Payments.Any(p => p.BankAccount.Id == id))
            {
                return BadRequest(new { message = $"This Bank Account {account.AccountTitle} is in use and it can not be deleted." });
            }
            if (account == null)
            {
                return NotFound(new { message = $"Bank Account with id {id} not found." });
            }
            _dbContext.BankAccounts.Remove(account);
            _dbContext.SaveChanges();
            return Ok(account);
        }
    }
}
