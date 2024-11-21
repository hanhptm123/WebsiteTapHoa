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
    public class ReceiptDetailsController : Controller
    {
        private readonly TaphoaContext _context;

        public ReceiptDetailsController(TaphoaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ViewReceiptDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var chiTietPhieuNhaps = await _context.Ctphieunhaps
                .Where(ctpn => ctpn.MapnNavigation.Mapn == id)  
                .Include(ctpn => ctpn.MapnNavigation)  
                .Include(ctpn => ctpn.MaspNavigation)  
                .ToListAsync();
            if (chiTietPhieuNhaps == null || !chiTietPhieuNhaps.Any())
            {
                return NotFound();
            }
            ViewBag.MaPhieuNhap = id;
            return View(chiTietPhieuNhaps);
        }


        private bool CtphieunhapExists(int id)
        {
            return _context.Ctphieunhaps.Any(e => e.Masp == id);
        }
    }
}
