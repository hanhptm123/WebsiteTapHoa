using Microsoft.AspNetCore.Mvc;
using TapHoa.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TapHoa.Models
{
    public class ProductRecipe : ViewComponent
    {
        private readonly TaphoaContext _context;

        public ProductRecipe(TaphoaContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke(int recipeId)
        {
            // Tìm công thức và lấy các sản phẩm đã liên kết
            var congthuc = _context.Congthucs
                .Where(c => c.Mact == recipeId)
                .Include(c => c.Masps)  // Bao gồm các sản phẩm liên quan
                .FirstOrDefault();

            if (congthuc == null || congthuc.Masps == null)
            {
                return View("NoProducts");  // Nếu không có sản phẩm liên kết, có thể trả về view không có sản phẩm
            }

            return View(congthuc.Masps);  // Trả về danh sách các sản phẩm liên quan đến công thức
        }
    }
}
