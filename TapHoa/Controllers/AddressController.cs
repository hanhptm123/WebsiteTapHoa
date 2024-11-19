using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;

namespace TapHoa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Only allow authenticated users
    public class AddressController : ControllerBase
    {
        private readonly TapHoaContext _context;

        public AddressController(TapHoaContext context)
        {
            _context = context;
        }

        // Get list of Provinces/Regions
        [HttpGet("GetProvinces")]
        public IActionResult GetProvinces()
        {
            var provinces = _context.Provinces
                .Select(p => new { p.Id, p.Name })
                .ToList();
            return Ok(provinces);
        }

        // Get list of Districts by Province/Region
        [HttpGet("GetDistricts/{provinceId}")]
        public IActionResult GetDistricts(int provinceId)
        {
            var districts = _context.Districts
                .Where(d => d.ProvinceId == provinceId)
                .Select(d => new { d.Id, d.Name })
                .ToList();
            return Ok(districts);
        }

        // Get list of Wards by District
        [HttpGet("GetWards/{districtId}")]
        public IActionResult GetWards(int districtId)
        {
            var wards = _context.Wards
                .Where(w => w.DistrictId == districtId)
                .Select(w => new { w.Id, w.Name })
                .ToList();
            return Ok(wards);
        }

        // Get list of customer addresses
        [HttpGet("GetAddresses")]
        public IActionResult GetAddresses()
        {
            // Get current user information
            var customerId = GetCurrentUserId();

            var addresses = _context.Diachis
                .Where(a => a.Makh == customerId)
                .Select(a => new
                {
                    a.Madc,
                    a.Tennguoinhan,
                    a.Sdt,
                    a.Diachi1
                })
                .ToList();

            return Ok(addresses);
        }

        // Add a new address
        [HttpPost("AddAddress")]
        public IActionResult AddAddress([FromBody] Diachi address)
        {
            if (address == null)
            {
                return BadRequest("Invalid address information.");
            }

            var customerId = GetCurrentUserId();
            address.Makh = customerId;

            _context.Diachis.Add(address);
            _context.SaveChanges();

            return Ok("Address has been successfully added.");
        }

        // Update an existing address
        [HttpPut("UpdateAddress/{id}")]
        public IActionResult UpdateAddress(int id, [FromBody] Diachi updatedAddress)
        {
            var existingAddress = _context.Diachis.FirstOrDefault(a => a.Madc == id && a.Makh == GetCurrentUserId());

            if (existingAddress == null)
            {
                return NotFound("Address does not exist or you do not have permission to edit it.");
            }

            existingAddress.Tennguoinhan = updatedAddress.Tennguoinhan;
            existingAddress.Sdt = updatedAddress.Sdt;
            existingAddress.Diachi1 = updatedAddress.Diachi1;

            _context.Entry(existingAddress).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok("Address has been updated.");
        }

        // Delete an address
        [HttpDelete("DeleteAddress/{id}")]
        public IActionResult DeleteAddress(int id)
        {
            var address = _context.Diachis.FirstOrDefault(a => a.Madc == id && a.Makh == GetCurrentUserId());

            if (address == null)
            {
                return NotFound("Address does not exist or you do not have permission to delete it.");
            }

            _context.Diachis.Remove(address);
            _context.SaveChanges();

            return Ok("Address has been deleted.");
        }

        // Method to get the current user ID
        private int GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                // Assume User.Identity.Name contains the CustomerId
                return int.Parse(User.Identity.Name ?? "0");
            }
            throw new UnauthorizedAccessException("User is not logged in.");
        }
    }
}
