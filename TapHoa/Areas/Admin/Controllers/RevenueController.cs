using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;

namespace TapHoa.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới truy cập được
    public class RevenueController : Controller
    {
        private readonly TapHoaContext _context;

        public RevenueController(TapHoaContext context)
        {
            _context = context;
        }

        // Action 1: Tổng doanh thu
        [HttpGet]
        public async Task<IActionResult> TotalRevenue()
        {
            var totalRevenue = await _context.Hoadons
                .Where(hd => hd.Ngaythanhtoan != null)
                .SumAsync(hd => hd.Chitiethoadons.Sum(cthd => cthd.Soluong * cthd.Dongia));

            return View(totalRevenue);
        }

        // Action 2: Doanh thu theo thời gian
        [HttpGet]
        public async Task<IActionResult> RevenueByTime(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                startDate = DateTime.Now.AddDays(-30); // Mặc định 30 ngày gần nhất
                endDate = DateTime.Now;
            }

            var revenue = await _context.Hoadons
                .Where(hd => hd.Ngaythanhtoan >= startDate && hd.Ngaythanhtoan <= endDate)
                .Select(hd => new
                {
                    hd.Mahd,
                    hd.Ngaythanhtoan,
                    Total = hd.Chitiethoadons.Sum(cthd => cthd.Soluong * cthd.Dongia)
                })
                .ToListAsync();

            return View(revenue);
        }

        // Action 3: Chi tiết doanh thu theo hóa đơn
        [HttpGet]
        public async Task<IActionResult> RevenueDetails(int mahd)
        {
            var invoice = await _context.Hoadons
                .Include(hd => hd.Chitiethoadons)
                .Include(hd => hd.MaptvcNavigation)
                .Include(hd => hd.ManvNavigation)
                .FirstOrDefaultAsync(hd => hd.Mahd == mahd);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // Action 4: Thống kê doanh thu từng khách hàng
        [HttpGet]
        public async Task<IActionResult> RevenueByCustomer()
        {
            var revenueByCustomer = await _context.Dondathangs
                .GroupBy(ddh => ddh.Makh)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    CustomerName = g.First().MakhNavigation.Tenkh,
                    TotalRevenue = g.Sum(ddh => ddh.Tonggia ?? 0)
                })
                .ToListAsync();

            return View(revenueByCustomer);
        }
    }
}