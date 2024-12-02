using Microsoft.AspNetCore.Mvc;
using TapHoa.Data;
using TapHoa.Models;
using TapHoa.Helpers;
namespace TapHoa.Controllers
{
    public class CartController : Controller
    {
        private readonly TaphoaContext _context;

        public CartController(TaphoaContext context)
        {
            _context = context;
        }
        const string CART_KEY = "MYCART";
        private int? GetCurrentAccountId()
        {
            var matk = HttpContext.Session.GetInt32("NewCustomerId");
             return matk;
        }
        public List<CartItem> Cart
        {
            get
            {
                var makh = GetCurrentAccountId();
                if (makh == null)
                {
                    return new List<CartItem>();
                }
                var cart = HttpContext.Session.Get<List<CartItem>>($"{CART_KEY}_{makh}") ?? new List<CartItem>();
                return cart;
            }
            set
            {
                var makh = GetCurrentAccountId();
                if (makh != null)
                {
                    HttpContext.Session.Set($"{CART_KEY}_{makh}", value);
                }
            }
        }
        private void SaveCart(List<CartItem> cart)
        {
            var makh = GetCurrentAccountId();
            if (makh != null)
            {
                HttpContext.Session.Set($"{CART_KEY}_{makh}", cart);
            }
        }
        public IActionResult Index()
        {
            var matk = GetCurrentAccountId();
            if (matk == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Login", "Accounts");
            }

            return View(Cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int quantity)
        {
            if (GetCurrentAccountId() == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm." });
            }

            var product = _context.Sanphams.SingleOrDefault(p => p.Masp == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            if (product.Soluong < quantity)
            {
                return Json(new { success = false, message = $"Số lượng sản phẩm vượt quá số lượng trong kho. Chỉ còn {product.Soluong} sản phẩm." });
            }

            var cart = Cart;
            var item = cart.SingleOrDefault(p => p.Masanpham == id);

            if (item == null)
            {
                var discount = (double?)_context.Khuyenmais
                 .Where(k => k.Makm == product.Makm)
                 .Select(k => k.Phantramgiam)
                 .FirstOrDefault() ?? 0.0;
                item = new CartItem
                {
                    Masanpham = product.Masp,
                    Tensanpham = product.Tensp,
                    Hinhanh = product.Hinhanh,
                    Dongia = (double)product.Gia,
                    Soluong = quantity,
                    Discount = (double)discount
                };
                cart.Add(item);
            }
            else
            {
                if (item.Soluong + quantity > product.Soluong)
                {
                    return Json(new { success = false, message = $"Không thể thêm {quantity} sản phẩm. Chỉ còn {product.Soluong - item.Soluong} sản phẩm trong kho." });
                }
                item.Soluong += quantity;
            }

            SaveCart(cart);

            return Json(new
            {
                success = true,
                cartData = new
                {
                    totalQuantity = cart.Sum(i => i.Soluong),
                    totalPrice = cart.Sum(i => i.Soluong * (i.Dongia * (1 - i.Discount / 100))).ToString("C")
                }
            });
        }
        public IActionResult RemoveFromCart(int id)
        {
            var makh = GetCurrentAccountId();
            if (makh == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để quản lý giỏ hàng.";
                return RedirectToAction("Login", "Accounts");
            }
            var cart = Cart;
            var item = cart.SingleOrDefault(p => p.Masanpham == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart); 
                TempData["SuccessMessage"] = "Đã xoá sản phẩm khỏi giỏ hàng.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng.";
            }
            return RedirectToAction("Index");
        }

    }

}
