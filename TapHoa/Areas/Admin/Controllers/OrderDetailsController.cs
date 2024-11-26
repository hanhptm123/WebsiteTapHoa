using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;

namespace TapHoa.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin")]
    [Route("admin/orderdetails")]
    public class OrderDetailsController : Controller
    {
        private readonly TaphoaContext _context;

        public OrderDetailsController(TaphoaContext context)
        {
            _context = context;
        }




        //Feature:View order details for admin
        [Route("ViewOrderDetails")]
        public async Task<IActionResult> ViewOrderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var chiTietDonDatHangs = await _context.Chitietdondathangs
                .Where(ctddh => ctddh.MaddhNavigation.Maddh == id)
                .Include(ctddh => ctddh.MaddhNavigation) 
                .Include(ctddh => ctddh.MaspNavigation)
                .ToListAsync();

            if (chiTietDonDatHangs == null || !chiTietDonDatHangs.Any())
            {
                return NotFound();
            }
            ViewBag.MaDonDatHang = id; 
            return View(chiTietDonDatHangs);
        }
        private bool ChitietdondathangExists(int id)
        {
            return _context.Chitietdondathangs.Any(e => e.Masp == id);
        }
    }
}
