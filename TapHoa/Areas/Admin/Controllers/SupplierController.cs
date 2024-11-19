using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;
using X.PagedList.Extensions;

namespace TapHoa.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/Supplier")]
    public class SupplierController : Controller
    {
        private readonly TaphoaContext _context;
        public SupplierController(TaphoaContext context)
        {
            _context = context;
        }
        [Route("supplierlist")]
        public IActionResult SupplierList(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;
            var lstncc = _context.Nhacungcaps.AsNoTracking().OrderBy(x => x.Tenncc).ToPagedList(pageNumber, pageSize);
            return View(lstncc);
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
        public IActionResult Create(Nhacungcap nhaCc)
        {
            if (ModelState.IsValid)
            {
                _context.Nhacungcaps.Add(nhaCc);
                _context.SaveChanges();
                TempData["Message"] = "Supplier created successfully";
                return RedirectToAction("SupplierList");
            }
            return View(nhaCc);
        }
        [Route("Edit")]
        [HttpGet]
        public IActionResult Edit(int Mancc)
        {
            var nhaCc = _context.Nhacungcaps.Find(Mancc);
            return View(nhaCc);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Nhacungcap nhaCc)
        {
            if (ModelState.IsValid)
            {
                _context.Update(nhaCc);
                _context.SaveChanges();
                return RedirectToAction("SupplierList");
            }
            return View(nhaCc);
        }
        [Route("Delete")]
        [HttpGet]
        public IActionResult Delete(int Mancc)
        {
            var nhaCc = _context.Nhacungcaps.Find(Mancc);

            if (nhaCc == null)
            {
                return NotFound();
            }

            var hasRelatedRecords = _context.Sanphams.Any(ct => ct.Mancc == Mancc);
            if (hasRelatedRecords)
            {
                TempData["ErrorMessage"] = "Cannot delete the supplier product because it has related records.";
                return RedirectToAction("SupplierList");
            }

            _context.Nhacungcaps.Remove(nhaCc);
            TempData["Message"] = "The supplier has been successfully deleted.";
            _context.SaveChanges();

            return RedirectToAction("SupplierList");
        }
    }
}
