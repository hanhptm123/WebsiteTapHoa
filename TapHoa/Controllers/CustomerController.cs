using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;
using X.PagedList;
using X.PagedList.Extensions;

namespace TapHoa.Areas.Admin.Controllers
{
    [Route("customer")]
    public class CustomerController : Controller
    {
        private readonly TaphoaContext _context;

        public CustomerController(TaphoaContext context)
        {
            _context = context;
        }
        [Route("index")]
        public IActionResult Index()
        {
            // Lấy matk từ session
            int? matk = HttpContext.Session.GetInt32("Matk");
            if (matk == null)
            {
                TempData["ErrorMessage"] = "No account found in session. Please log in.";
                return RedirectToAction("Login", "Accounts");
            }

            // Truy xuất makh và các thuộc tính khách hàng
            var khachhang = _context.Khachhangs.FirstOrDefault(kh => kh.Matk == matk);
            if (khachhang == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Login", "Accounts");
            }

            // Trả về view hiển thị thông tin khách hàng
            return View(khachhang);
        }
        [Route("EditCustomer")]
        public IActionResult EditCustomer()
        {
            // Lấy matk từ session
            int? matk = HttpContext.Session.GetInt32("Matk");
            if (matk == null)
            {
                TempData["ErrorMessage"] = "No account found in session. Please log in.";
                return RedirectToAction("Login", "Accounts");
            }

            // Truy xuất thông tin khách hàng
            var khachhang = _context.Khachhangs.FirstOrDefault(kh => kh.Matk == matk);
            if (khachhang == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Index", "Home");
            }

            // Trả về view để chỉnh sửa thông tin khách hàng
            return View(khachhang);
        }

        [HttpPost]
        public IActionResult EditCustomer(int makh, string Tenkh, string Email, string Sdt, string Diachi)
        {
            if (string.IsNullOrEmpty(Tenkh) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Sdt) || string.IsNullOrEmpty(Diachi))
            {
                TempData["ErrorMessage"] = "Please fill in all fields.";
                return RedirectToAction("EditCustomer");
            }

            // Truy xuất khách hàng cần chỉnh sửa
            var khachhang = _context.Khachhangs.FirstOrDefault(kh => kh.Makh == makh);
            if (khachhang == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Index", "Home");
            }

            // Cập nhật thông tin khách hàng
            khachhang.Tenkh = Tenkh;
            khachhang.Email = Email;
            khachhang.Sdt = Sdt;
            khachhang.Diachi = Diachi;

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Customer information updated successfully!";
            return RedirectToAction("Index", "Accounts");
        }
        private int? GetCurrentAccountId()
        {
            // Lấy mã tài khoản từ session
            return HttpContext.Session.GetInt32("NewCustomerId");
        }

        private int? GetCustomerIdFromAccountId()
        {
            var matk = GetCurrentAccountId();
            if (!matk.HasValue)
                return null;

            // Truy xuất makh từ bảng tài khoản (nếu có liên kết với khách hàng)
            var makh = _context.Khachhangs
                .Where(kh => kh.Matk == matk.Value)
                .Select(kh => kh.Makh)
                .FirstOrDefault();

            return makh;
        }
        [Route("OrderList")]
        public IActionResult OrderList()
        {
            var makh = GetCustomerIdFromAccountId();
            if (!makh.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Login", "Accounts");
            }

            // Truy vấn danh sách đơn hàng dựa trên makh
            var orders = _context.Dondathangs
                .Where(dh => dh.Makh == makh.Value)
                .Include(dh => dh.Chitietdondathangs)
                .ThenInclude(ct => ct.MaspNavigation)
                .Include(o => o.MattddhNavigation)
                .OrderByDescending(dh => dh.Ngaydat)
                .ToList();


            return View(orders);
        }

        public IActionResult OrderDetails(int id)
        {
            var makh = GetCustomerIdFromAccountId();
            if (!makh.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Login", "Accounts");
            }

            // Lấy thông tin đơn hàng
            var order = _context.Dondathangs
                .Include(dh => dh.MattddhNavigation) // Nạp thông tin trạng thái đơn hàng
                .FirstOrDefault(dh => dh.Makh == makh.Value && dh.Maddh == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại hoặc bạn không có quyền xem.";
                return RedirectToAction("OrderList");
            }

            // Lấy thông tin chi tiết đơn hàng
            var orderDetails = _context.Chitietdondathangs
                .Include(ct => ct.MaspNavigation) // Nạp thông tin sản phẩm
                .Where(ct => ct.Maddh == id)
                .ToList();

            // Sử dụng ViewBag để truyền dữ liệu cho view
            ViewBag.Order = order; // Truyền thông tin đơn hàng
            ViewBag.OrderDetails = orderDetails; // Truyền danh sách chi tiết đơn hàng

            return View();
        }
    }
}
