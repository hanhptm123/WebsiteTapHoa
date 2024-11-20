using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;
using X.PagedList.Extensions;

namespace TapHoa.Controllers
{
    public class BlogController : Controller
    {
        private readonly TaphoaContext _context;
        public BlogController(TaphoaContext context)
        {
            _context = context;
        }

        public IActionResult BlogList(int? page)
        {
            int pageSize = 3;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;
            var lstbv = _context.Baiviets.AsNoTracking().OrderBy(x => x.Mabv).ToPagedList(pageNumber, pageSize);
            return View(lstbv);
        }
    }
}
