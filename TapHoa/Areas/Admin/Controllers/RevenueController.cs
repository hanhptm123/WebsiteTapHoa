using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;

namespace TapHoa.Controllers
{
    public class RevenueController : Controller
    {
        private readonly TaphoaContext _context;

        public RevenueController(TaphoaContext context)
        {
            _context = context;
        }

        // GET: Revenue
        public async Task<IActionResult> Index()
        {
            var taphoaContext = _context.Dondathangs.Include(d => d.MakhNavigation).Include(d => d.MaptvcNavigation).Include(d => d.MattddhNavigation);
            return View(await taphoaContext.ToListAsync());
        }

        // GET: Revenue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dondathang = await _context.Dondathangs
                .Include(d => d.MakhNavigation)
                .Include(d => d.MaptvcNavigation)
                .Include(d => d.MattddhNavigation)
                .FirstOrDefaultAsync(m => m.Maddh == id);
            if (dondathang == null)
            {
                return NotFound();
            }

            return View(dondathang);
        }

        // GET: Revenue/Create
        public IActionResult Create()
        {
            ViewData["Makh"] = new SelectList(_context.Khachhangs, "Makh", "Makh");
            ViewData["Maptvc"] = new SelectList(_context.Phuongthucvanchuyens, "Maptvc", "Maptvc");
            ViewData["Mattddh"] = new SelectList(_context.Trangthaidondathangs, "Mattddh", "Mattddh");
            return View();
        }

        // POST: Revenue/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Maddh,Mattddh,Maptvc,Tenkh,Diachi,Sdt,Ngaydat,Tonggia,Lydohuy,Makh")] Dondathang dondathang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dondathang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Makh"] = new SelectList(_context.Khachhangs, "Makh", "Makh", dondathang.Makh);
            ViewData["Maptvc"] = new SelectList(_context.Phuongthucvanchuyens, "Maptvc", "Maptvc", dondathang.Maptvc);
            ViewData["Mattddh"] = new SelectList(_context.Trangthaidondathangs, "Mattddh", "Mattddh", dondathang.Mattddh);
            return View(dondathang);
        }

        // GET: Revenue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dondathang = await _context.Dondathangs.FindAsync(id);
            if (dondathang == null)
            {
                return NotFound();
            }
            ViewData["Makh"] = new SelectList(_context.Khachhangs, "Makh", "Makh", dondathang.Makh);
            ViewData["Maptvc"] = new SelectList(_context.Phuongthucvanchuyens, "Maptvc", "Maptvc", dondathang.Maptvc);
            ViewData["Mattddh"] = new SelectList(_context.Trangthaidondathangs, "Mattddh", "Mattddh", dondathang.Mattddh);
            return View(dondathang);
        }

        // POST: Revenue/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Maddh,Mattddh,Maptvc,Tenkh,Diachi,Sdt,Ngaydat,Tonggia,Lydohuy,Makh")] Dondathang dondathang)
        {
            if (id != dondathang.Maddh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dondathang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DondathangExists(dondathang.Maddh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Makh"] = new SelectList(_context.Khachhangs, "Makh", "Makh", dondathang.Makh);
            ViewData["Maptvc"] = new SelectList(_context.Phuongthucvanchuyens, "Maptvc", "Maptvc", dondathang.Maptvc);
            ViewData["Mattddh"] = new SelectList(_context.Trangthaidondathangs, "Mattddh", "Mattddh", dondathang.Mattddh);
            return View(dondathang);
        }

        // GET: Revenue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dondathang = await _context.Dondathangs
                .Include(d => d.MakhNavigation)
                .Include(d => d.MaptvcNavigation)
                .Include(d => d.MattddhNavigation)
                .FirstOrDefaultAsync(m => m.Maddh == id);
            if (dondathang == null)
            {
                return NotFound();
            }

            return View(dondathang);
        }

        // POST: Revenue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dondathang = await _context.Dondathangs.FindAsync(id);
            if (dondathang != null)
            {
                _context.Dondathangs.Remove(dondathang);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DondathangExists(int id)
        {
            return _context.Dondathangs.Any(e => e.Maddh == id);
        }
        public IActionResult TongDoanhThuTheoThang(int year, int month)
        {
            // Kiểm tra nếu năm và tháng được cung cấp hợp lệ
            if (year <= 0 || month <= 0 || month > 12)
            {
                return BadRequest("Năm hoặc tháng không hợp lệ.");
            }

            // Tính ngày đầu tiên và cuối cùng của tháng được chỉ định
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            // Truy vấn các đơn đặt hàng có trạng thái 'Đã giao' trong khoảng thời gian đó
            var dondathangs = _context.Dondathangs
                .Include(d => d.MattddhNavigation) // Include the status navigation
                .Where(d => d.Ngaydat >= startDate && d.Ngaydat <= endDate && d.MattddhNavigation.Tenttddh == "Delivered")
                .ToList();

            // Tính tổng doanh thu từ các đơn đặt hàng đã giao
            decimal tongDoanhThu = dondathangs.Sum(d => d.Tonggia ?? 0);

            // Trả về view kèm theo dữ liệu tổng doanh thu và danh sách đơn hàng đã giao
            ViewBag.Thang = $"{month}/{year}";
            return View("TongDoanhThuTheoThang", new TongDoanhThuViewModel
            {
                TongDoanhThu = tongDoanhThu,
                DanhSachDonDatHang = dondathangs
            });
        }

    }
}
