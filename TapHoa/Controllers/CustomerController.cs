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

            int? matk = HttpContext.Session.GetInt32("Matk");
            if (matk == null)
            {
                TempData["ErrorMessage"] = "No account found in session. Please log in.";
                return RedirectToAction("Login", "Accounts");
            }

            var khachhang = _context.Khachhangs.FirstOrDefault(kh => kh.Matk == matk);
            if (khachhang == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Login", "Accounts");
            }

            return View(khachhang);
        }
        [Route("EditCustomer")]
        public IActionResult EditCustomer()
        {

            int? matk = HttpContext.Session.GetInt32("Matk");
            if (matk == null)
            {
                TempData["ErrorMessage"] = "No account found in session. Please log in.";
                return RedirectToAction("Login", "Accounts");
            }


            var khachhang = _context.Khachhangs.FirstOrDefault(kh => kh.Matk == matk);
            if (khachhang == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Index", "Home");
            }

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


            var khachhang = _context.Khachhangs.FirstOrDefault(kh => kh.Makh == makh);
            if (khachhang == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Index", "Home");
            }


            khachhang.Tenkh = Tenkh;
            khachhang.Email = Email;
            khachhang.Sdt = Sdt;
            khachhang.Diachi = Diachi;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Customer information updated successfully!";
            return RedirectToAction("Index", "Accounts");
        }

        [Route("OrderList")]
        public IActionResult OrderList()
        {
            var matk = HttpContext.Session.GetInt32("NewCustomerId");
            if (!matk.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Login", "Accounts");
            }

            var customer = _context.Khachhangs.Find(matk.Value);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Login", "Accounts");
            }

            var orders = _context.Dondathangs
                .Where(dh => dh.Makh == customer.Makh)
                .Include(dh => dh.Chitietdondathangs)
                    .ThenInclude(ct => ct.MaspNavigation)
                .Include(dh => dh.MattddhNavigation)
                .OrderByDescending(dh => dh.Ngaydat)
                .ToList();

            return View(orders);
        }

        [Route("OrderDetails/{id}")]
        public IActionResult OrderDetails(int id)
        {
            var matk = HttpContext.Session.GetInt32("NewCustomerId");
            if (!matk.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Login", "Accounts");
            }
            var order = _context.Dondathangs
                .Include(dh => dh.MattddhNavigation)
                .FirstOrDefault(dh => dh.Makh == matk.Value && dh.Maddh == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại hoặc bạn không có quyền xem.";
                return RedirectToAction("OrderList");
            }
            var orderDetails = _context.Chitietdondathangs
                .Where(ct => ct.Maddh == id)
                .Include(ct => ct.MaspNavigation)
                .ToList();

            ViewBag.Order = order;
            ViewBag.OrderDetails = orderDetails;

            return View();
        }

    }
}
