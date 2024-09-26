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
    public class VendorController : ControllerBase
    {
        private LgmsDbContext _dbContext;
        private PagedData<Vendor> _pagedData;

        public VendorController(LgmsDbContext dbContext)
        {
            _dbContext = dbContext;
            _pagedData = new PagedData<Vendor>();
        }

        [HttpGet("GetVendors")]
        public IActionResult GetVendors()
        {
            var vendors = _dbContext.Vendors.ToList();

            return Ok(vendors);
        }

        [HttpPost("GetVendorsWithFilters")]
        public IActionResult GetVendorsWithFilters(VendorsSearchModel vendorSearchModel)
        {
            if (vendorSearchModel == null) return BadRequest("Invalid search criteria");
            var vendors = new List<Vendor>();
            try
            {
                vendors = _dbContext.Vendors.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (!vendors.Any()) return NotFound("Vendors Not Found");

            if (!string.IsNullOrEmpty(vendorSearchModel.SearchDetails.SearchTerm))
            {
                vendors = vendors.Where(v =>
                    v.Name.ToUpper().Contains(vendorSearchModel.SearchDetails.SearchTerm.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(vendorSearchModel.SortDetails.SortColumn) && vendorSearchModel.SortDetails.SortDirection != Enum.SortDirections.None)
            {
                switch (vendorSearchModel.SortDetails.SortColumn)
                {
                    case "id":
                        vendors = vendorSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            vendors.OrderBy(e => e.Id).ToList() :
                            vendors.OrderByDescending(e => e.Id).ToList();
                        break;
                    case "name":
                        vendors = vendorSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            vendors.OrderBy(e => e.Name).ToList() :
                            vendors.OrderByDescending(e => e.Name).ToList();
                        break;
                    default:
                        vendors = vendorSearchModel.SortDetails.SortDirection == Enum.SortDirections.Ascending ?
                            vendors.OrderBy(e => e.Name).ToList() :
                            vendors.OrderByDescending(e => e.Name).ToList();
                        break;
                }
            }
            else
            {
                vendors = vendors.OrderBy(e => e.Id).ToList();
            }
            var pagedVendorsResult = _pagedData.GetPagedData(
                vendors,
                (PagedDataRequestModel)vendorSearchModel.PaginationDetails
            );

            return Ok(pagedVendorsResult);
        }
    }
}
