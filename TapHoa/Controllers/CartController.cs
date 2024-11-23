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

        // Lấy mã tài khoản hiện tại từ session
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

                // Lấy giỏ hàng từ session theo mã khách hàng
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
                // Lưu giỏ hàng vào session với key theo mã khách hàng
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

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var makh = GetCurrentAccountId();
            if (makh == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Login", "Accounts");
            }

            var cart = Cart;
            var item = cart.SingleOrDefault(p => p.Masanpham == id);

            if (item == null)
            {
                var sanpham = _context.Sanphams.SingleOrDefault(p => p.Masp == id);
                var khuyenmai = _context.Khuyenmais.SingleOrDefault(k => k.Makm == id);
                if (sanpham == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy sản phẩm với mã {id}.";
                    return RedirectToAction("Index");
                }

                item = new CartItem
                {
                    Masanpham = sanpham.Masp,
                    Tensanpham = sanpham.Tensp,
                    Hinhanh = sanpham.Hinhanh,
                    Dongia = (double)sanpham.Gia,
                    Soluong = quantity,
                    // Handle possible null value for khuyenmai
                    Discount = khuyenmai.Phantramgiam // If khuyenmai is null, default to 0
                };
                cart.Add(item);
            }
            else
            {
                item.Soluong += quantity;
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";
            return RedirectToAction("Index");
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
                SaveCart(cart); // Save the updated cart
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
