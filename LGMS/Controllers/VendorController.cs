﻿using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Dto;
using LGMS.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LGMS.Controllers
{
    [Authorize(Roles = "Stores")]
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
        public IActionResult GetVendorsWithFilters(BaseSearchModel vendorSearchModel)
        {
            if (vendorSearchModel == null) return BadRequest(new { message = "Invalid search criteria" });
            var vendors = new List<Vendor>();
            try
            {
                vendors = _dbContext.Vendors.ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }

            //if (!vendors.Any()) return NotFound(new { message = "No vendors are there" });

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

        [HttpGet("GetVendorById")]
        public IActionResult GetVendorById(int id)
        {
            var vendor = _dbContext.Vendors
                .SingleOrDefault(d => d.Id == id);
            if (vendor == null) return BadRequest(new { message = string.Format("Vendor with id {0} doesn't exist", id) });
            return Ok(vendor);
        }

        [HttpPost("EditVendor")]
        public IActionResult EditVendor(VendorEditModel vendorDetails)
        {
            var existingVendor = _dbContext.Vendors.FirstOrDefault(d => d.Id == vendorDetails.Id);

            if (existingVendor == null)
            {
                return NotFound(new { message = "Vendor not Found" });
            }

            if (vendorDetails.Location != null && _dbContext.Vendors.Any(d => d.Name.ToUpper() == vendorDetails.Name.ToUpper() && d.Location.ToUpper() == vendorDetails.Location.ToUpper() && d.Id != vendorDetails.Id))
            {
                return BadRequest(new
                {
                    message = "Vendor with this Name already Exist"
                });
            }
            try
            {
                existingVendor.Name = vendorDetails.Name;
                existingVendor.Location = vendorDetails.Location;   
                _dbContext.SaveChanges();
                return Ok(existingVendor);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [HttpPost("DeleteVendor")]
        public IActionResult DeleteVendor([FromBody] int id)
        {
            var existingVendor = _dbContext.Vendors.FirstOrDefault(d => d.Id == id);

            if (existingVendor == null)
            {
                return NotFound(new { message = "Vendor not Found" });
            }
            if (_dbContext.Equipments.Any(e => e.Vendor.Id == existingVendor.Id))
            {
                return BadRequest(new { message = $"{existingVendor.Name} is in use and it can't be delete." });
            }

            try
            {
                _dbContext.Vendors.Remove(existingVendor);
                _dbContext.SaveChanges();
                return Ok(new { message = $"{existingVendor.Name} has been deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
                });
            }
        }

        [HttpPost("AddVendors")]
        public IActionResult AddVendors([FromBody] List<string> vendorsWithLocation)
        { 
            var vendorData = vendorsWithLocation
                                .Select(vendor => vendor.Split('|'))
                                .Where(parts => parts.Length == 1 || parts.Length == 2)
                                .Select(parts => new
                                {
                                    Name = parts[0].Trim(),
                                    Location = parts.Length == 2 ? parts[1].Trim() : null
                                })
                                .ToList();

            var vendorNames = vendorData.Select(v => v.Name).ToList();
            var existingVendors = _dbContext.Vendors
                                            .Where(v => vendorNames.Select(n => n.ToLower()).Contains(v.Name.ToLower()))
                                            .Select(v => v.Name)
                                            .ToList();

            if (existingVendors.Any())
            {
                return BadRequest(new { message = $"The following vendors already exist: {string.Join(", ", existingVendors)}" });
            }

            var newVendors = vendorData.Select(v => new Vendor { Name = v.Name, Location = v.Location }).ToList();
            _dbContext.Vendors.AddRange(newVendors);
            _dbContext.SaveChanges();

            return Ok(new { message = $"{newVendors.Count} vendors added successfully." });
        }


    }
}
