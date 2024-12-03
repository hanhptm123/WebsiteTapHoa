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
        private const string CART_KEY = "MYCART";

        public CheckoutController(TaphoaContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            var makh = GetCurrentAccountId();
            if (makh == null)
            {
                return new List<CartItem>();
            }

            return HttpContext.Session.Get<List<CartItem>>($"{CART_KEY}_{makh}") ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var makh = GetCurrentAccountId();
            if (makh != null)
            {
                HttpContext.Session.Set($"{CART_KEY}_{makh}", cart);
            }
        }

        private int? GetCurrentAccountId()
        {
            return HttpContext.Session.GetInt32("NewCustomerId");
        }

        public IActionResult Index()
        {
            var cartItems = GetCart();
            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
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
            var cartItems = GetCart();
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var matk = HttpContext.Session.GetInt32("NewCustomerId");
            if (!matk.HasValue)
            {
                return RedirectToAction("Login", "Account");
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
                    Makh = matk.Value,
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

                    var sanpham = _context.Sanphams.FirstOrDefault(sp => sp.Masp == cartItem.Masanpham);
                    if (sanpham == null || sanpham.Soluong < cartItem.Soluong)
                    {
                        throw new Exception($"Sản phẩm {sanpham?.Tensp ?? "không tồn tại"} không đủ số lượng.");
                    }

                    sanpham.Soluong -= cartItem.Soluong;
                }

                _context.SaveChanges();
                HttpContext.Session.Set("Maddh", dondathang.Maddh);

                transaction.Commit();
                HttpContext.Session.Remove($"{CART_KEY}_{matk.Value}");

                return RedirectToAction("OrderSuccess");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError("", "Đặt hàng thất bại: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult CancelOrder(string lydohuy)
        {
            var maddh = HttpContext.Session.Get<int>("Maddh");

            if (maddh == 0)
            {
                TempData["CancelMessage"] = "Không tìm thấy mã đơn đặt hàng trong session.";
                return RedirectToAction("OrderDetails");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var dondathang = _context.Dondathangs
                    .Include(ddh => ddh.Chitietdondathangs)
                    .FirstOrDefault(ddh => ddh.Maddh == maddh);

                if (dondathang == null || dondathang.Mattddh != 1)
                {
                    TempData["CancelMessage"] = "Đơn hàng không tồn tại hoặc không thể hủy.";
                    return RedirectToAction("OrderDetails");
                }

                dondathang.Mattddh = 3;
                dondathang.Lydohuy = lydohuy;
                _context.Dondathangs.Update(dondathang);

                foreach (var chitiet in dondathang.Chitietdondathangs)
                {
                    var sanpham = _context.Sanphams.FirstOrDefault(sp => sp.Masp == chitiet.Masp);
                    if (sanpham != null)
                    {
                        sanpham.Soluong += chitiet.Soluong;
                        _context.Sanphams.Update(sanpham);
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                TempData["CancelMessage"] = "Hủy đơn hàng thành công.";
                return RedirectToAction("OrderDetails");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["CancelMessage"] = "Hủy đơn hàng thất bại: " + ex.Message;
                return RedirectToAction("OrderDetails");
            }
        }

        public IActionResult OrderDetails()
        {
            var maddh = HttpContext.Session.Get<int>("Maddh");

            if (maddh == 0)
            {
                return NotFound("Không tìm thấy mã đơn đặt hàng trong session.");
            }

            var dondathang = _context.Dondathangs
                .Include(ddh => ddh.Chitietdondathangs)
                    .ThenInclude(ct => ct.MaspNavigation)
                .FirstOrDefault(ddh => ddh.Maddh == maddh);

            if (dondathang == null)
            {
                return NotFound("Đơn đặt hàng không tồn tại.");
            }

            return View(dondathang);
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
