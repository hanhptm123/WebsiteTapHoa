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
            // Lấy danh sách sản phẩm
            var products = await _context.Sanphams.ToListAsync();
            ViewBag.Products = products; // Gửi danh sách sản phẩm đến View

            if (products == null || !products.Any())
            {
                // Nếu không có sản phẩm nào, có thể chuyển hướng hoặc chỉ cảnh báo
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
                // Tạo đối tượng công thức mới
                var newRecipe = new Congthuc
                {
                    Ten = Name,
                    Video = VideoUrl
                };

                // Thêm công thức mới vào cơ sở dữ liệu
                _context.Congthucs.Add(newRecipe);
                await _context.SaveChangesAsync();

                // Thêm các sản phẩm đã chọn vào công thức
                if (SelectedProducts != null && SelectedProducts.Any())
                {
                    foreach (var masp in SelectedProducts)
                    {
                        var sanpham = await _context.Sanphams.FindAsync(masp);
                        if (sanpham != null)
                        {
                            // Liên kết sản phẩm với công thức
                            newRecipe.Masps.Add(sanpham);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, nạp lại danh sách sản phẩm để hiển thị lại view
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                .Include(c => c.Masps) // Lấy các sản phẩm liên kết
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
                .Include(c => c.Masps) // Lấy các sản phẩm liên kết
                .FirstOrDefaultAsync(m => m.Mact == id);

            if (congthuc != null)
            {
                // Ngắt liên kết giữa công thức và các sản phẩm
                foreach (var sanpham in congthuc.Masps.ToList())
                {
                    // Loại bỏ công thức khỏi danh sách Macts của sản phẩm
                    sanpham.Macts.Remove(congthuc); // Xóa công thức khỏi sản phẩm
                }

                // Xóa công thức khỏi cơ sở dữ liệu
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
                .Include(c => c.Masps) // Ensure to include the products related to the recipe
                .FirstOrDefaultAsync(m => m.Mact == id);

            if (congthuc == null)
            {
                return NotFound();
            }

            return View(congthuc);
        }



    }
}
