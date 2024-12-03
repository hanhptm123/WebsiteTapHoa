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
    public class DailyRecipeController : Controller
    {
        private readonly TaphoaContext _context;

        public DailyRecipeController(TaphoaContext context)
        {
            _context = context;
        }

        // GET: DailyRecipe
        public async Task<IActionResult> Index()
        {
            return View(await _context.Congthucs.ToListAsync());
        }

        // GET: DailyRecipe/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var congthuc = await _context.Congthucs
                .FirstOrDefaultAsync(m => m.Mact == id);
            if (congthuc == null)
            {
                return NotFound();
            }

            return View(congthuc);
        }

        public async Task<IActionResult> Create()
        {
            var products = await _context.Sanphams.ToListAsync();
            ViewBag.Products = products; 

            if (products == null || !products.Any())
            {
                
                ViewBag.ErrorMessage = "No products available. Please add products before creating a recipe.";
            }

            return View();
        }


        // POST: DailyRecipe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Name, string VideoUrl, List<int> SelectedProducts)
        {
            if (ModelState.IsValid)
            {
                var newRecipe = new Congthuc
                {
                    Ten = Name,
                    Video = VideoUrl
                };

                _context.Congthucs.Add(newRecipe);
                await _context.SaveChangesAsync();

                if (SelectedProducts != null && SelectedProducts.Any())
                {
                    foreach (var masp in SelectedProducts)
                    {
                        var sanpham = await _context.Sanphams.FindAsync(masp);
                        if (sanpham != null)
                        {
                            newRecipe.Masps.Add(sanpham);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = await _context.Sanphams.ToListAsync();
            return View();
        }

        // GET: DailyRecipe/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var congthuc = await _context.Congthucs.FindAsync(id);
            if (congthuc == null)
            {
                return NotFound();
            }
            return View(congthuc);
        }

        // POST: DailyRecipe/Edit/5
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Mact,Ten,Video")] Congthuc congthuc)
        {
            if (id != congthuc.Mact)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(congthuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CongthucExists(congthuc.Mact))
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
            return View(congthuc);
        }

        // GET: DailyRecipe/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var congthuc = await _context.Congthucs
                .Include(c => c.Masps) 
                .FirstOrDefaultAsync(m => m.Mact == id);

            if (congthuc == null)
            {
                return NotFound();
            }

            return View(congthuc);
        }

        // POST: DailyRecipe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var congthuc = await _context.Congthucs
                .Include(c => c.Masps) 
                .FirstOrDefaultAsync(m => m.Mact == id);

            if (congthuc != null)
            {
                
                foreach (var sanpham in congthuc.Masps.ToList())
                {
                    
                    sanpham.Macts.Remove(congthuc); 
                }

                
                _context.Congthucs.Remove(congthuc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool CongthucExists(int id)
        {
            return _context.Congthucs.Any(e => e.Mact == id);
        }
        public async Task<IActionResult> ViewDailyRecipe()
        {
            var recipes = await _context.Congthucs.ToListAsync();
            return View(recipes);
        }
        public async Task<IActionResult> DetailsCustomer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var congthuc = await _context.Congthucs
                .Include(c => c.Masps) 
                .FirstOrDefaultAsync(m => m.Mact == id);

            if (congthuc == null)
            {
                return NotFound();
            }

            return View(congthuc);
        }



    }
}
