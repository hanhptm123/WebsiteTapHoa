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

        // Lấy mã tài khoản hiện tại từ session
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

        // Hiển thị danh sách yêu thích
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

            var likes = Likes; // Lấy danh sách yêu thích hiện tại

            if (likes.Any(l => l.Masanpham == id))
            {
                return Json(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích." });
            }

            likes.Add(new LikeItem
            {
                Masanpham = product.Masp,
                Tensanpham = product.Tensp,
                Hinhanh = product.Hinhanh
            });

            Likes = likes; // Cập nhật lại session

            return Json(new { success = true, message = "Đã thêm sản phẩm vào danh sách yêu thích." });
        }
    }
}
