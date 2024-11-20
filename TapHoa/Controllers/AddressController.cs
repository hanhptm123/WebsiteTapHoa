using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;
using X.PagedList;
using X.PagedList.Extensions;

namespace TapHoa.Areas.Admin.Controllers
{
    [Route("address")]
    public class AddressController : Controller
    {
        private readonly TaphoaContext _context;

        public AddressController(TaphoaContext context)
        {
            _context = context;
        }

        [Route("AddressList")]
        public IActionResult AddressList(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;
            var addresses = _context.Diachis.AsNoTracking().OrderBy(x => x.Madc).ToPagedList(pageNumber, pageSize);
            return View(addresses);
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
        public IActionResult Create(Diachi address)
        {
            if (ModelState.IsValid)
            {
                _context.Diachis.Add(address);
                _context.SaveChanges();
                TempData["Message"] = "Address created successfully";
                return RedirectToAction("AddressList");
            }
            return View(address);
        }

        [Route("Edit")]
        [HttpGet]
        public IActionResult Edit(int Madc)
        {
            var address = _context.Diachis.Find(Madc);
            if (address == null)
            {
                return NotFound();
            }
            return View(address);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Diachi address)
        {
            if (ModelState.IsValid)
            {
                _context.Update(address);
                _context.SaveChanges();
                TempData["Message"] = "Address updated successfully";
                return RedirectToAction("AddressList");
            }
            return View(address);
        }

        [Route("Delete")]
        [HttpGet]
        public IActionResult Delete(int Madc)
        {
            var address = _context.Diachis.Find(Madc);

            if (address == null)
            {
                return NotFound();
            }

            _context.Diachis.Remove(address);
            _context.SaveChanges();
            TempData["Message"] = "Address deleted successfully";
            return RedirectToAction("AddressList");
        }
    }
}
