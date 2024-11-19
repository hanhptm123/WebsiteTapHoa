using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;
using X.PagedList.Extensions;

namespace TapHoa.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/brand")]
    public class BrandController : Controller
    {
        private readonly TaphoaContext _context;
        public BrandController(TaphoaContext context)
        {
            _context = context;
        }
        [Route("brandlist")]
        public IActionResult BrandList(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;
            var lstTH = _context.Thuonghieus.AsNoTracking().OrderBy(x => x.Tenthuonghieu).ToPagedList(pageNumber, pageSize);
            return View(lstTH);
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
        public IActionResult Create(Thuonghieu TH)
        {
            if (ModelState.IsValid)
            {
                _context.Thuonghieus.Add(TH);
                _context.SaveChanges();
                TempData["Message"] = "Brand created successfully";
                return RedirectToAction("BrandList");
            }
            return View(TH);
        }
        
        [Route("Edit")]
        [HttpGet]
        public IActionResult Edit(int Mathuonghieu)
        {
            var TH = _context.Thuonghieus.AsNoTracking().FirstOrDefault(th => th.Mathuonghieu == Mathuonghieu);
            return View(TH);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Thuonghieu TH)
        {
            if (ModelState.IsValid)
            {
                _context.Update(TH);
                _context.SaveChanges();
                return RedirectToAction("BrandList");
            }
            return View(TH);
        }
        [Route("Delete")]
        [HttpGet]
        public IActionResult Delete(int Mathuonghieu)
        {
            var TH = _context.Thuonghieus.Find(Mathuonghieu);

            if (TH == null)
            {
                return NotFound();
            }

            var hasRelatedRecords = _context.Sanphams.Any(ct => ct.Math == Mathuonghieu);
            if (hasRelatedRecords)
            {
                TempData["ErrorMessage"] = "Cannot delete the brand product because it has related records.";
                return RedirectToAction("BrandList");
            }

            _context.Thuonghieus.Remove(TH);
            _context.SaveChanges();
            TempData["Message"] = "The brand has been successfully deleted.";

            return RedirectToAction("BrandList");
        }
    }
}
