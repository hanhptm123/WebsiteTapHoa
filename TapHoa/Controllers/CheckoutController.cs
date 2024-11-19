using Microsoft.AspNetCore.Mvc;
using TapHoa.Data;
using TapHoa.Helpers;
using TapHoa.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;

namespace TapHoa.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly TaphoaContext _context;

        public CheckoutController(TaphoaContext context)
        {
            _context = context;
        }

        public List<Cartitem> Carts
        {
            get
            {
                var data = HttpContext.Session.Get<List<Cartitem>>("GioHang");
                return data ?? new List<Cartitem>();
            }
        }

        public IActionResult Index()
        {
            var cartItems = Carts;
            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "CartItem");
            }
            ViewBag.PaymentMethods = _context.Phuongthucthanhtoans.ToList();
            ViewBag.ShippingOptions = _context.Phuongthucvanchuyens.ToList();
            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = cartItems.Sum(item => item.Giasaugiam * item.Soluong);
            ViewBag.Total = cartItems.Sum(item => item.Giasaugiam * item.Soluong);

            return View();
        }

        [HttpPost]
        public IActionResult PlaceOrder(string tenkh, string diachi, string sdt, int mapttt, int maptvc)
        {
            var cartItems = Carts;
            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "CartItem");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var dondathang = new Dondathang
                {
                    Tenkh = tenkh,
                    Diachi = diachi,
                    Sdt = sdt,
                    Ngaydat = DateTime.Now,
                    Mapttt = mapttt,
                    Maptvc = maptvc,
                    Tonggia = cartItems.Sum(item => (decimal)(item.Giasaugiam * item.Soluong)),
                    Makh = 2,
                    Mattddh = 1
                };

                _context.Dondathangs.Add(dondathang);
                _context.SaveChanges();

                foreach (var cartItem in cartItems)
                {
                    var chitiet = new Chitietdondathang
                    {
                        Maddh = dondathang.Maddh,
                        Masp = cartItem.Masanpham,
                        Soluong = cartItem.Soluong,
                        Thanhtien = (decimal)(cartItem.Giasaugiam * cartItem.Soluong)
                    };

                    _context.Chitietdondathangs.Add(chitiet);
                }

                _context.SaveChanges();
                transaction.Commit();
                HttpContext.Session.Remove("GioHang");

                return RedirectToAction("OrderSuccess");
            }
            catch
            {
                transaction.Rollback();
                return RedirectToAction("Index");
            }
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
