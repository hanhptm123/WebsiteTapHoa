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
    public class ProductController : Controller
    {
        private readonly TaphoaContext _context;
        public ProductController(TaphoaContext context)
        {
            _context = context;
        }
        private bool SanphamExists(int id)
        {
            return _context.Sanphams.Any(e => e.Masp == id);
        }
        public async Task<IActionResult> ViewProduct()
        {
            var taphoaContext = await _context.Sanphams
                .Include(s => s.MakmNavigation)
                .Include(s => s.MaloaispNavigation)
                .Include(s => s.ManccNavigation)
                .Include(s => s.MathNavigation)
                .ToListAsync(); 
            return View(taphoaContext);
        }
        public async Task<IActionResult> ProductByCategory(int? id)
        {
            var products = await _context.Sanphams
                .Include(s => s.MakmNavigation) 
                .Include(s => s.MaloaispNavigation)
                .Include(s => s.ManccNavigation)
                .Include(s => s.MathNavigation)
                .Where(p => p.Maloaisp == id) 
                .ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanpham = await _context.Sanphams
                .Include(s => s.MakmNavigation)
                .Include(s => s.MaloaispNavigation)
                .Include(s => s.ManccNavigation)
                .Include(s => s.MathNavigation)
                .FirstOrDefaultAsync(m => m.Masp == id);

            if (sanpham == null)
            {
                return NotFound();
            }

            return View(sanpham);
        }
        [HttpGet]
        public IActionResult Search(string keyword)
        {
            var products = string.IsNullOrEmpty(keyword)
                ? new List<Sanpham>() 
                : _context.Sanphams
                          .Where(sp => sp.Tensp.Contains(keyword))
                          .ToList();
            return PartialView("ProductList", products);
        }

    }
}
