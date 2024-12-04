using Microsoft.AspNetCore.Mvc;
using TapHoa.Data;
using TapHoa.Models;
using TapHoa.Helpers;

namespace TapHoa.Controllers
{
    public class LikeController : Controller
    {
        private readonly TaphoaContext _context;
        const string LIKE_KEY = "MYLIKE";

        public LikeController(TaphoaContext context)
        {
            _context = context;
        }

        private int? GetCurrentAccountId()
        {
            return HttpContext.Session.GetInt32("NewCustomerId");
        }

        public List<LikeItem> Likes
        {
            get
            {
                var matk = GetCurrentAccountId();
                if (matk == null)
                {
                    return new List<LikeItem>();
                }

                var likes = HttpContext.Session.Get<List<LikeItem>>($"{LIKE_KEY}_{matk}") ?? new List<LikeItem>();
                return likes;
            }
            set
            {
                var matk = GetCurrentAccountId();
                if (matk != null)
                {
                    HttpContext.Session.Set($"{LIKE_KEY}_{matk}", value);
                }
            }
        }

        public IActionResult Index()
        {
            var matk = GetCurrentAccountId();
            if (matk == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem danh sách yêu thích.";
                return RedirectToAction("Login", "Accounts");
            }

            return View(Likes);
        }

        [HttpPost]
        public IActionResult AddToLike(int id)
        {
            if (GetCurrentAccountId() == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào danh sách yêu thích." });
            }

            var product = _context.Sanphams.SingleOrDefault(p => p.Masp == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            var likes = Likes;

            if (likes.Any(l => l.Masanpham == id))
            {
                return Json(new { success = false, message = "The product is already in the favorites list." });
            }

            likes.Add(new LikeItem
            {
                Masanpham = product.Masp,
                Tensanpham = product.Tensp,
                Hinhanh = product.Hinhanh
            });

            Likes = likes;

            return Json(new { success = true, message = "The product has been added to the favorites list." });
        }
        public IActionResult RemoveFromLike(int id)
        {
            var makh = GetCurrentAccountId();
            if (makh == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để quản lý danh sách yêu thích.";
                return RedirectToAction("Login", "Accounts");
            }

            var likes = Likes;  
            var item = likes.SingleOrDefault(p => p.Masanpham == id);

            if (item != null)
            {
                likes.Remove(item);  
                Likes = likes; 
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi danh sách yêu thích.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong danh sách yêu thích.";
            }

            return RedirectToAction("Index");
        }


    }
}
