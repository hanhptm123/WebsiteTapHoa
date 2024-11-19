using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;

namespace TapHoa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Chỉ cho phép người dùng đã đăng nhập
    public class AddressController : ControllerBase
    {
        private readonly TapHoaContext _context;

        public AddressApiController(TapHoaContext context)
        {
            _context = context;
        }

        // Lấy danh sách Tỉnh/Thành phố
        [HttpGet("GetProvinces")]
        public IActionResult GetProvinces()
        {
            var provinces = _context.Provinces
                .Select(p => new { p.Id, p.Name })
                .ToList();
            return Ok(provinces);
        }

        // Lấy danh sách Quận/Huyện theo Tỉnh/Thành phố
        [HttpGet("GetDistricts/{provinceId}")]
        public IActionResult GetDistricts(int provinceId)
        {
            var districts = _context.Districts
                .Where(d => d.ProvinceId == provinceId)
                .Select(d => new { d.Id, d.Name })
                .ToList();
            return Ok(districts);
        }

        // Lấy danh sách Phường/Xã theo Quận/Huyện
        [HttpGet("GetWards/{districtId}")]
        public IActionResult GetWards(int districtId)
        {
            var wards = _context.Wards
                .Where(w => w.DistrictId == districtId)
                .Select(w => new { w.Id, w.Name })
                .ToList();
            return Ok(wards);
        }

        // Lấy danh sách địa chỉ của khách hàng
        [HttpGet("GetAddresses")]
        public IActionResult GetAddresses()
        {
            // Lấy thông tin người dùng hiện tại
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

        // Thêm địa chỉ mới
        [HttpPost("AddAddress")]
        public IActionResult AddAddress([FromBody] Diachi address)
        {
            if (address == null)
            {
                return BadRequest("Thông tin địa chỉ không hợp lệ.");
            }

            var customerId = GetCurrentUserId();
            address.Makh = customerId;

            _context.Diachis.Add(address);
            _context.SaveChanges();

            return Ok("Địa chỉ đã được thêm thành công.");
        }

        // Cập nhật địa chỉ
        [HttpPut("UpdateAddress/{id}")]
        public IActionResult UpdateAddress(int id, [FromBody] Diachi updatedAddress)
        {
            var existingAddress = _context.Diachis.FirstOrDefault(a => a.Madc == id && a.Makh == GetCurrentUserId());

            if (existingAddress == null)
            {
                return NotFound("Địa chỉ không tồn tại hoặc bạn không có quyền sửa.");
            }

            existingAddress.Tennguoinhan = updatedAddress.Tennguoinhan;
            existingAddress.Sdt = updatedAddress.Sdt;
            existingAddress.Diachi1 = updatedAddress.Diachi1;

            _context.Entry(existingAddress).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok("Địa chỉ đã được cập nhật.");
        }

        // Xóa địa chỉ
        [HttpDelete("DeleteAddress/{id}")]
        public IActionResult DeleteAddress(int id)
        {
            var address = _context.Diachis.FirstOrDefault(a => a.Madc == id && a.Makh == GetCurrentUserId());

            if (address == null)
            {
                return NotFound("Địa chỉ không tồn tại hoặc bạn không có quyền xóa.");
            }

            _context.Diachis.Remove(address);
            _context.SaveChanges();

            return Ok("Địa chỉ đã được xóa.");
        }

        // Phương thức để lấy ID của người dùng hiện tại
        private int GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                // Giả sử User.Identity.Name chứa CustomerId
                return int.Parse(User.Identity.Name ?? "0");
            }
            throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");
        }
    }
}
