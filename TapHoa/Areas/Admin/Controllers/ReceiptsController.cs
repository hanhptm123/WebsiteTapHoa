using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;

namespace TapHoa.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/receipts")]
    public class ReceiptsController : Controller
    {
        private readonly TaphoaContext _context;

        public ReceiptsController(TaphoaContext context)
        {
            _context = context;
        }
        [Route("Index")]
        // GET: Admin/Receipts
        public async Task<IActionResult> Index()
        {
            var taphoaContext = _context.Phieunhaps.Include(p => p.ManvNavigation);
            return View(await taphoaContext.ToListAsync());
        }

        // GET: Admin/Receipts/Create
        [Route("Create")]
        public IActionResult Create()
        {
            ViewData["Manv"] = new SelectList(_context.Nhanviens, "Manv", "Manv");
            ViewBag.Products = _context.Sanphams.ToList(); // Pass products to the view
            return View();
        }

        // POST: Admin/Receipts/Create
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<Ctphieunhap> receiptDetails)
        {
            if (receiptDetails == null || !receiptDetails.Any())
            {
                TempData["Error"] = "The receipt details list cannot be empty.";
                return RedirectToAction("Index");  // Chuyển hướng về Index nếu không có chi tiết phiếu nhập
            }

            try
            {
                int? employeeId = HttpContext.Session.GetInt32("NhanvienId");
                if (!employeeId.HasValue)
                {
                    TempData["Error"] = "Employee ID is missing. Please log in again.";
                    return RedirectToAction("Login", "Accounts");  // Nếu không có mã nhân viên thì quay về trang login
                }

                var newReceipt = new Phieunhap
                {
                    Manv = employeeId.Value,
                    Ngaynhap = DateTime.Now,
                    Tongtien = 0 // Tổng tiền sẽ được tính sau
                };

                _context.Phieunhaps.Add(newReceipt);
                await _context.SaveChangesAsync();

                int receiptId = newReceipt.Mapn;
                decimal totalAmount = 0;
                List<string> errorMessages = new();

                foreach (var detail in receiptDetails)
                {
                    if (detail.Soluong <= 0 || detail.Gia <= 0)
                    {
                        errorMessages.Add($"Invalid quantity or price for product ID {detail.Masp}.");
                        continue;
                    }

                    var product = await _context.Sanphams.FirstOrDefaultAsync(p => p.Masp == detail.Masp);
                    if (product == null)
                    {
                        errorMessages.Add($"Product with ID {detail.Masp} not found.");
                        continue;
                    }

                    var receiptDetail = new Ctphieunhap
                    {
                        Mapn = receiptId,
                        Masp = detail.Masp,
                        Soluong = detail.Soluong,
                        Gia = detail.Gia,
                        Thanhtien = detail.Soluong * detail.Gia.Value
                    };

                    totalAmount += receiptDetail.Thanhtien;

                    product.Soluong = (product.Soluong ?? 0) + detail.Soluong;

                    _context.Ctphieunhaps.Add(receiptDetail);
                }

                if (errorMessages.Any())
                {
                    TempData["Error"] = string.Join("<br>", errorMessages);
                    return RedirectToAction("Index");
                }

                newReceipt.Tongtien = totalAmount;
                _context.Update(newReceipt);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Receipt created successfully!";
                return RedirectToAction("Index");  // Đảm bảo chuyển hướng về trang Index sau khi tạo thành công
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");  // Nếu có lỗi, quay lại trang Index
            }
        }

    }
}
