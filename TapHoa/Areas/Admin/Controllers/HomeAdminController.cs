﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;
using X.PagedList;
using X.PagedList.Extensions;

namespace TapHoa.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/homeadmin")]
    public class HomeAdminController : Controller
    {
        private readonly TaphoaContext _context;

        public HomeAdminController(TaphoaContext context)
        {
            _context = context;
        }
        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("productlist")]
        public IActionResult ProductList(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;
            var lstsanpham = _context.Sanphams.AsNoTracking().OrderBy(x => x.Tensp).ToPagedList(pageNumber, pageSize);
            return View(lstsanpham);
        }
        [Route("CreateProduct")]
        [HttpGet]
        public IActionResult CreateProduct()
        {
            ViewBag.Mathuonghieu = new SelectList(_context.Thuonghieus.ToList(), "Mathuonghieu", "Thuonghieu");
            ViewBag.Mancc = new SelectList(_context.Nhacungcaps.ToList(), "Mancc", "Tenncc");
            ViewBag.Maloaisp = new SelectList(_context.Loaisps.ToList(), "Maloaisp", "Tenloaisp");
            ViewBag.Makm = new SelectList(_context.Khuyenmais.ToList(), "Makm", "Phantramgiam");
            return View();
        }
        [Route("CreateProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(Sanpham sanPham)
        {
            if (ModelState.IsValid)
            {
                _context.Sanphams.Add(sanPham);
                _context.SaveChanges();
                TempData["Message"] = "Product created successfully";
                return RedirectToAction("ProductList");
            }
            return View(sanPham);
        }
        [Route("EditProduct")]
        [HttpGet]
        public IActionResult EditProduct(int Masp)
        {
            ViewBag.Mathuonghieu = new SelectList(_context.Thuonghieus.ToList(), "Mathuonghieu", "Thuonghieu");
            ViewBag.Mancc = new SelectList(_context.Nhacungcaps.ToList(), "Mancc", "Tenncc");
            ViewBag.Maloaisp = new SelectList(_context.Loaisps.ToList(), "Maloaisp", "Tenloaisp");
            ViewBag.Makm = new SelectList(_context.Khuyenmais.ToList(), "Makm", "Phantramgiam");
            var sanPham = _context.Sanphams.Find(Masp);
            return View(sanPham);
        }
        [Route("EditProduct")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(Sanpham sanPham)
        {
            if (ModelState.IsValid)
            {
                _context.Update(sanPham);
                _context.SaveChanges();
                return RedirectToAction("ProductList", "HomeAdmin");
            }
            return View(sanPham);
        }
        [Route("DeleteProduct")]
        [HttpGet]
        public IActionResult DeleteProduct(int Masp)
        {
            var sanPham = _context.Sanphams.Find(Masp);

            if (sanPham == null)
            {
                return NotFound();
            }

            var hasRelatedRecords = _context.Ctphieunhaps.Any(ct => ct.Masp == Masp);
            if (hasRelatedRecords)
            {
                TempData["ErrorMessage"] = "Cannot delete the product because it has related records.";
                return RedirectToAction("ProductList", "HomeAdmin");
            }

            _context.Sanphams.Remove(sanPham);
            _context.SaveChanges();
            TempData["Message"] = "The product has been successfully deleted.";

            return RedirectToAction("ProductList", "HomeAdmin");
        }
        [Route("danhmucloaisanpham")]
        public IActionResult DanhMucLoaiSanPham(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 1 ? 1 : page.Value;
            var lstloaisanpham = _context.Loaisps.AsNoTracking().OrderBy(x => x.Tenloaisp).ToPagedList(pageNumber, pageSize);
            return View(lstloaisanpham);
        }
        [Route("ThemLoai")]
        [HttpGet]
        public IActionResult ThemLoai()
        {
            return View();
        }

        [Route("ThemLoai")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemLoai(Loaisp lSanPham)
        {
            if (ModelState.IsValid)
            {
                var existingLoai = _context.Loaisps.FirstOrDefault(x => x.Tenloaisp == lSanPham.Tenloaisp);
                if (existingLoai != null)
                {
                    ModelState.AddModelError("Tenloaisp", "Loại sản phẩm này đã tồn tại.");
                    return View(lSanPham);
                }

                _context.Loaisps.Add(lSanPham);
                _context.SaveChanges();
                return RedirectToAction("DanhMucLoaiSanPham");
            }
            return View(lSanPham);
        }
        [Route("SuaLoai")]
        [HttpGet]
        public IActionResult SuaLoai(int Maloaisp)
        {
            var lSanPham = _context.Loaisps.Find(Maloaisp);

            if (lSanPham == null)
            {
                return NotFound();
            }

            return View(lSanPham);
        }

        [Route("SuaLoai")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaLoai([Bind("Maloaisp,Tenloaisp")] Loaisp lSanPham)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lSanPham);
                    _context.SaveChanges();
                    return RedirectToAction("DanhMucLoaiSanPham", "HomeAdmin");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu: " + ex.Message);
                }
            }
            return View(lSanPham);
        }
        [Route("XoaLoai")]
        [HttpGet]
        public IActionResult XoaLoai(int Maloaisp)
        {
            var lSanPham = _context.Loaisps.Find(Maloaisp);

            if (lSanPham == null)
            {
                return NotFound();
            }

            var hasRelatedRecords = _context.Sanphams.Any(ct => ct.Maloaisp == Maloaisp);
            if (hasRelatedRecords)
            {
                TempData["ErrorMessage"] = "Không thể xóa loại sản phẩm vì có bản ghi liên quan.";
                return RedirectToAction("DanhMucLoaiSanPham", "HomeAdmin");
            }

            _context.Loaisps.Remove(lSanPham);
            _context.SaveChanges();

            return RedirectToAction("DanhMucLoaiSanPham", "HomeAdmin");
        }


    }
}
