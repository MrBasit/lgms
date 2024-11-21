using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        LgmsDbContext _dbContext;
        PagedData<Client> _pagedData;
        public ClientController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Client>();
        }

        [HttpPost("GetClientsWithFilters")]
        public IActionResult GetClientsWithFilters(BaseSearchModel searchModel)
        {
            if (searchModel == null) return BadRequest(new { message = "Invalid search criteria" });

            var clients = new List<Client>();

            try
            {
                clients = _dbContext.Clients.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
            if (!clients.Any()) return NotFound(new { message = "No clients are there" });


            if (!string.IsNullOrEmpty(searchModel.SearchDetails.SearchTerm))
            {
                clients = clients.Where(e =>
                    e.Name.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Number.ToUpper().Contains(searchModel.SearchDetails.SearchTerm.ToUpper()) ||
                    e.Phone.Contains(searchModel.SearchDetails.SearchTerm)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.SortDetails.SortColumn) &&
                searchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (searchModel.SortDetails.SortColumn)
                {
                    case "number":
                        clients = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    clients.OrderBy(e => e.Number).ToList() :
                                    clients.OrderByDescending(e => e.Number).ToList();
                        break;
                    case "name":
                        clients = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    clients.OrderBy(e => e.Name).ToList() :
                                    clients.OrderByDescending(e => e.Name).ToList();
                        break;
                    case "business":
                        clients = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    clients.OrderBy(e => e.Business).ToList() :
                                    clients.OrderByDescending(e => e.Business).ToList();
                        break;
                    default:
                        clients = searchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                                    clients.OrderBy(e => e.Number).ToList() :
                                    clients.OrderByDescending(e => e.Number).ToList();
                        break;
                }
            }
            else
            {
                clients = clients.OrderBy(e => e.Number).ToList();
            }

            var pagedClientsResult = _pagedData.GetPagedData(
                clients,
                (PagedDataRequestModel)searchModel.PaginationDetails
            );

            return Ok(pagedClientsResult);
        }

        [HttpGet("GetClients")]
        public IActionResult GetClients()
        {
            var clients = _dbContext.Clients.ToList();
            return Ok(clients);
        }

        [HttpGet("GetClientById")]
        public IActionResult GetClientById(int clientId)
        {
            var client = _dbContext.Clients.SingleOrDefault(e => e.Id == clientId);
            if (client == null)  return BadRequest(new {message = string.Format("Client with id {0} doesn't exist", clientId) });
            return Ok(client);

        }

        [HttpPost("AddClient")]
        public IActionResult AddClient(ClientsAddModel details)
        {
            if (_dbContext.Clients.Any(e => e.Phone == details.Phone))
            {
                return BadRequest(new { message = "Client with this Phone number already exists." });
            }
            if (details.Email != null && _dbContext.Clients.Any(e => e.Email.ToUpper() == details.Email.ToUpper()))
            {
                return BadRequest(new { message = "Client with this Email already exists." });
            }
            try
            {
                string clientNumber = GenerateClientNumber();
                Client client = new Client()
                {
                    Number = clientNumber,
                    Name = details.Name,
                    Phone = details.Phone,
                    Email = details.Email,
                    Location = details.Location,
                    Business = details.Business
                };

                _dbContext.Clients.Add(client);
                _dbContext.SaveChanges();
                return Ok(client);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [HttpPost("EditClient")]
        public ActionResult EditClient(ClientEditModel details)
        {
            var existingClient = _dbContext.Clients
                .FirstOrDefault(e => e.Id == details.Id);

            if (existingClient == null)
            {
                return NotFound(new { message = "Client not found" });
            }
            if (details.Email != null && _dbContext.Clients.Any(e => e.Email.ToUpper() == details.Email.ToUpper() && e.Id != details.Id))
            {
                return BadRequest(new { message = "Another client with this Email already exists" });
            }
            if (_dbContext.Clients.Any(e => e.Phone == details.Phone && e.Id != details.Id))
            {
                return BadRequest(new { message = "Another client with this Phone number already exists" });
            }

            try
            {
                existingClient.Name = details.Name;
                existingClient.Business = details.Business;
                existingClient.Email = details.Email;
                existingClient.Location = details.Location;
                existingClient.Phone = details.Phone;

                _dbContext.SaveChanges();

                return Ok(existingClient);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        private string GenerateClientNumber()
        {
            var lastClient = _dbContext.Clients
                .OrderByDescending(c => c.Number)
                .FirstOrDefault();

            if (lastClient == null)
            {
                return "CLN0001";
            }

            var lastClientNumber = lastClient.Number;
            var numberPart = lastClientNumber.Substring(3);
            var nextNumber = (int.Parse(numberPart) + 1).ToString();
            return "CLN" + nextNumber.PadLeft(numberPart.Length, '0');
        }



    }
}
