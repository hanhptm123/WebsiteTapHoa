using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;
using X.PagedList;
using X.PagedList.Extensions;

namespace TapHoa.Areas.Admin.Controllers
{
    [Route("customer")]
    public class CustomerController : Controller
    {
        private readonly TaphoaContext _context;

        public CustomerController(TaphoaContext context)
        {
            _context = context;
        }

        [Route("CustomerList")]
        public IActionResult CustomerList(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;
            var customers = _context.Khachhangs.AsNoTracking().OrderBy(x => x.Makh).ToPagedList(pageNumber, pageSize);
            return View(customers);
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
        public IActionResult Create(Khachhang customer)
        {
            if (ModelState.IsValid)
            {
                _context.Khachhangs.Add(customer);
                _context.SaveChanges();
                TempData["Message"] = "Customer created successfully";
                return RedirectToAction("CustomerList");
            }
            return View(customer);
        }

        [Route("Edit")]
        [HttpGet]
        public IActionResult Edit(int Makh)
        {
            var customer = _context.Khachhangs.Find(Makh);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Khachhang customer)
        {
            if (ModelState.IsValid)
            {
                _context.Update(customer);
                _context.SaveChanges();
                TempData["Message"] = "Customer updated successfully";
                return RedirectToAction("CustomerList");
            }
            return View(customer);
        }

        [Route("Delete")]
        [HttpGet]
        public IActionResult Delete(int Makh)
        {
            var customer = _context.Khachhangs.Find(Makh);

            if (customer == null)
            {
                return NotFound();
            }

            _context.Khachhangs.Remove(customer);
            _context.SaveChanges();
            TempData["Message"] = "Customer deleted successfully";
            return RedirectToAction("CustomerList");
        }
    }
}
