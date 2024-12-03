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
            var customerId = GetCurrentAccountId();
            if (customerId == null)
            {
                return new List<CartItem>();
            }

            return HttpContext.Session.Get<List<CartItem>>($"{CART_KEY}_{customerId}") ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var customerId = GetCurrentAccountId();
            if (customerId != null)
            {
                HttpContext.Session.Set($"{CART_KEY}_{customerId}", cart);
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
        public IActionResult PlaceOrder(string customerName, string address, string phone, int paymentMethodId, int shippingMethodId)
        {
            var cartItems = GetCart();
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var accountId = HttpContext.Session.GetInt32("NewCustomerId");
            if (!accountId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var order = new Dondathang
                {
                    Tenkh = customerName,
                    Diachi = address,
                    Sdt = phone,
                    Ngaydat = DateTime.Now,
                    Mapttt = paymentMethodId,
                    Maptvc = shippingMethodId,
                    Tonggia = cartItems.Sum(item => (decimal)(item.Giasaugiam * item.Soluong)),
                    Makh = accountId.Value,
                    Mattddh = 1
                };

                _context.Dondathangs.Add(order);
                _context.SaveChanges();

                foreach (var cartItem in cartItems)
                {
                    var orderDetail = new Chitietdondathang
                    {
                        Maddh = order.Maddh,
                        Masp = cartItem.Masanpham,
                        Soluong = cartItem.Soluong,
                        Thanhtien = (decimal)(cartItem.Giasaugiam * cartItem.Soluong)
                    };

                    _context.Chitietdondathangs.Add(orderDetail);

                    var product = _context.Sanphams.FirstOrDefault(sp => sp.Masp == cartItem.Masanpham);
                    if (product == null || product.Soluong < cartItem.Soluong)
                    {
                        throw new Exception($"Product {product?.Tensp ?? "does not exist"} does not have enough quantity.");
                    }

                    product.Soluong -= cartItem.Soluong;
                }

                _context.SaveChanges();
                HttpContext.Session.Set("Maddh", order.Maddh);

                transaction.Commit();
                HttpContext.Session.Remove($"{CART_KEY}_{accountId.Value}");

                return RedirectToAction("OrderSuccess");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError("", "Order placement failed: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult CancelOrder(string cancellationReason)
        {
            var orderId = HttpContext.Session.Get<int>("Maddh");

            if (orderId == 0)
            {
                TempData["CancelMessage"] = "Order ID not found in session.";
                return RedirectToAction("OrderDetails");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var order = _context.Dondathangs
                    .Include(o => o.Chitietdondathangs)
                    .FirstOrDefault(o => o.Maddh == orderId);

                if (order == null || order.Mattddh != 1)
                {
                    TempData["CancelMessage"] = "The order does not exist or cannot be canceled.";
                    return RedirectToAction("OrderDetails");
                }

                order.Mattddh = 3;
                order.Lydohuy = cancellationReason;
                _context.Dondathangs.Update(order);

                foreach (var detail in order.Chitietdondathangs)
                {
                    var product = _context.Sanphams.FirstOrDefault(p => p.Masp == detail.Masp);
                    if (product != null)
                    {
                        product.Soluong += detail.Soluong;
                        _context.Sanphams.Update(product);
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                TempData["CancelMessage"] = "Order successfully canceled.";
                return RedirectToAction("OrderDetails");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["CancelMessage"] = "Order cancellation failed: " + ex.Message;
                return RedirectToAction("OrderDetails");
            }
        }

        public IActionResult OrderDetails()
        {
            var orderId = HttpContext.Session.Get<int>("Maddh");

            if (orderId == 0)
            {
                return NotFound("Order ID not found in session.");
            }

            var order = _context.Dondathangs
                .Include(o => o.Chitietdondathangs)
                    .ThenInclude(d => d.MaspNavigation)
                .FirstOrDefault(o => o.Maddh == orderId);

            if (order == null)
            {
                return NotFound("The order does not exist.");
            }

            return View(order);
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
