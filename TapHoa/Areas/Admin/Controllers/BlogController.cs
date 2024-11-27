using Microsoft.AspNetCore.Mvc;
using TapHoa.Data;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace TapHoa.Areas.Admin.Controllers
{

    [Area("admin")]
    [Route("admin")]
    [Route("admin/blog")]
    public class BlogController : Controller
    {
        private readonly TaphoaContext _context;
        public BlogController(TaphoaContext context)
        {
            _context = context;
        }
        [Route("bloglist")]
        public IActionResult BlogList(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;
            var lstbv = _context.Baiviets.AsNoTracking().OrderBy(x => x.Mabv).ToPagedList(pageNumber, pageSize);
            return View(lstbv);
        }
        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Baiviet baiviet)
        {
            if (ModelState.IsValid)
            {
                _context.Baiviets.Add(baiviet);
                _context.SaveChanges();
                TempData["Message"] = "Blog created successfully";
                return RedirectToAction("BlogList");
            }
            return View(baiviet);
        }

        [Route("Edit")]
        [HttpGet]
        public IActionResult Edit(int Mabv)
        {
            var baiviet = _context.Baiviets.AsNoTracking().FirstOrDefault(bv => bv.Mabv == Mabv);
            return View(baiviet);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Baiviet baiviet)
        {
            if (ModelState.IsValid)
            {
                _context.Update(baiviet);
                _context.SaveChanges();
                return RedirectToAction("BlogList");
            }
            return View(baiviet);
        }
        [Route("Delete")]
        [HttpGet]
        public IActionResult Delete(int Mabv)
        {
            var baiviet = _context.Baiviets.Find(Mabv);

            if (baiviet == null)
            {
                return NotFound();
            }
            _context.Baiviets.Remove(baiviet);
            _context.SaveChanges();
            TempData["Message"] = "The blog has been successfully deleted.";

            return RedirectToAction("BlogList");
        }
    }
}
