using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;

namespace TapHoa.Controllers
{
    public class EvaluateController : Controller
    {
        private readonly TaphoaContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EvaluateController(TaphoaContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Evaluate
        public async Task<IActionResult> Index()
        {
            var evaluateList = _context.Danhgia
                .Include(e => e.MakhNavigation)
                .Include(e => e.MaspNavigation);
            return View(await evaluateList.ToListAsync());
        }

        // GET: Evaluate/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evaluation = await _context.Danhgia
                .Include(e => e.MakhNavigation)
                .Include(e => e.MaspNavigation)
                .FirstOrDefaultAsync(e => e.Madg == id);

            if (evaluation == null)
            {
                return NotFound();
            }

            return View(evaluation);
        }

        // GET: Evaluate/Create
        public IActionResult Create(int? Masp)
        {
            int? customerId = HttpContext.Session.GetInt32("NewCustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Masp = Masp;
            ViewData["Makh"] = new SelectList(_context.Khachhangs, "Makh", "Tenkh", customerId);
            return View();
        }

        // POST: Evaluate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Madg,Masp,Makh,Ngaydanhgia,Loidanhgia,Diemdanhgia,Hinhanh")] Danhgium evaluation, IFormFile Hinhanh)
        {
            int? customerId = HttpContext.Session.GetInt32("NewCustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Check if the customer has already reviewed this product
            var existingReview = _context.Danhgia.FirstOrDefault(e => e.Masp == evaluation.Masp && e.Makh == customerId);
            if (existingReview != null)
            {
                ViewBag.ErrorMessage = "You have already reviewed this product!";
                return View(evaluation);
            }

            evaluation.Makh = customerId.Value;

            if (ModelState.IsValid)
            {
                // Save the image to the wwwroot/Image folder
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (Hinhanh != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(Hinhanh.FileName);
                    string extension = Path.GetExtension(Hinhanh.FileName);
                    evaluation.Hinhanh = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string path = Path.Combine(wwwRootPath + "/Image/", evaluation.Hinhanh);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await Hinhanh.CopyToAsync(fileStream);
                    }
                }

                _context.Add(evaluation);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Sanphams", new { id = evaluation.Masp });
            }

            ViewData["Makh"] = new SelectList(_context.Khachhangs, "Makh", "Tenkh", evaluation.Makh);
            return View(evaluation);
        }

        // GET: Evaluate/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evaluation = await _context.Danhgia.FindAsync(id);
            if (evaluation == null)
            {
                return NotFound();
            }

            ViewData["Makh"] = new SelectList(_context.Khachhangs, "Makh", "Tenkh", evaluation.Makh);
            ViewData["Masp"] = new SelectList(_context.Sanphams, "Masp", "Tensp", evaluation.Masp);
            return View(evaluation);
        }

        // POST: Evaluate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Madg,Masp,Makh,Ngaydanhgia,Loidanhgia,Diemdanhgia,Hinhanh")] Danhgium evaluation)
        {
            if (id != evaluation.Madg)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(evaluation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EvaluationExists(evaluation.Madg))
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

            ViewData["Makh"] = new SelectList(_context.Khachhangs, "Makh", "Tenkh", evaluation.Makh);
            ViewData["Masp"] = new SelectList(_context.Sanphams, "Masp", "Tensp", evaluation.Masp);
            return View(evaluation);
        }

        // GET: Evaluate/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evaluation = await _context.Danhgia
                .Include(e => e.MakhNavigation)
                .Include(e => e.MaspNavigation)
                .FirstOrDefaultAsync(e => e.Madg == id);

            if (evaluation == null)
            {
                return NotFound();
            }

            return View(evaluation);
        }

        // POST: Evaluate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evaluation = await _context.Danhgia.FindAsync(id);
            if (evaluation != null)
            {
                _context.Danhgia.Remove(evaluation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EvaluationExists(int id)
        {
            return _context.Danhgia.Any(e => e.Madg == id);
        }
    }
}
