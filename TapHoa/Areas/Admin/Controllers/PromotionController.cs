using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;
using X.PagedList.Extensions;

namespace TapHoa.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/Promotion")]
    public class PromotionController : Controller
    {
        private readonly TaphoaContext _context;
        public  PromotionController(TaphoaContext context)
        {
            _context = context;
        }
        [Route("promotionlist")]
        public IActionResult PromotionList(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;
            var lstkm = _context.Khuyenmais.AsNoTracking().OrderBy(x => x.Phantramgiam).ToPagedList(pageNumber, pageSize);
            return View(lstkm);
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
        public IActionResult Create(Khuyenmai khuyenMai)
        {
            if (ModelState.IsValid)
            {
                _context.Khuyenmais.Add(khuyenMai);
                _context.SaveChanges();
                TempData["Message"] = "Promotion created successfully";
                return RedirectToAction("PromotionList");
            }
            return View(khuyenMai);
        }
        [Route("Edit")]
        [HttpGet]
        public IActionResult Edit(int Makm)
        {
            var khuyenMai = _context.Khuyenmais.Find(Makm);
            return View(khuyenMai);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Khuyenmai khuyenMai)
        {
            if (ModelState.IsValid)
            {
                _context.Update(khuyenMai);
                _context.SaveChanges();
                return RedirectToAction("PromotionList");
            }
            return View(khuyenMai);
        }
        [Route("Delete")]
        [HttpGet]
        public IActionResult Delete(int Makm)
        {
            var khuyenMai = _context.Khuyenmais.Find(Makm);

            if (khuyenMai == null)
            {
                return NotFound();
            }

            var hasRelatedRecords = _context.Sanphams.Any(ct => ct.Makm == Makm);
            if (hasRelatedRecords)
            {
                TempData["ErrorMessage"] = "Cannot delete the promotion product because it has related records.";
                return RedirectToAction("PromotionList");
            }

            _context.Khuyenmais.Remove(khuyenMai);
            TempData["Message"] = "The promotion has been successfully deleted.";
            _context.SaveChanges();

            return RedirectToAction("PromotionList");
        }
    }
}
