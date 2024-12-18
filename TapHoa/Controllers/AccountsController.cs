using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TapHoa.Data;

namespace TapHoa.Controllers
{
    public class AccountsController : Controller
    {
        private readonly TaphoaContext _context;

        public AccountsController(TaphoaContext context)
        {
            _context = context;
        }

        private bool IsAccountExist(int accountId)
        {
            return _context.Taikhoans.Any(account => account.Matk == accountId);
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string Tendangnhap, string Matkhau)
        {
            var taikhoan = _context.Taikhoans.FirstOrDefault(t => t.Tendangnhap == Tendangnhap && t.Matkhau == Matkhau);
            if (taikhoan == null)
            {
                TempData["ErrorMessage"] = "Invalid username or password.";
                return RedirectToAction("Login");
            }
            HttpContext.Session.SetInt32("Matk", taikhoan.Matk);
            if (taikhoan.Chucvu == "Nhanvien")
            {
                var nhanvien = _context.Nhanviens.FirstOrDefault(nv => nv.Matk == taikhoan.Matk);
                if (nhanvien != null)
                {
                    HttpContext.Session.SetInt32("NhanvienId", nhanvien.Manv);
                }
                else
                {
                    TempData["ErrorMessage"] = "Employee account not found.";
                    return RedirectToAction("Login");
                }
            }
            else if (taikhoan.Chucvu == "Khachhang")
            {
                var khachhang = _context.Khachhangs.FirstOrDefault(kh => kh.Matk == taikhoan.Matk);
                if (khachhang != null)
                {
                    HttpContext.Session.SetInt32("NewCustomerId", khachhang.Makh);
                }
                else
                {
                    return RedirectToAction("RegisterCustomer", "Accounts", new { matk = taikhoan.Matk });

                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid role.";
                return RedirectToAction("Login");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, taikhoan.Tendangnhap),
                new Claim(ClaimTypes.Role, taikhoan.Chucvu),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            TempData["SuccessMessage"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("NewCustomerId");
            HttpContext.Session.Remove("NhanvienId");

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "You have logged out successfully.";
            return RedirectToAction("Login", "Accounts");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string Tendangnhap, string Matkhau, string MatkhauConfirm, string Chucvu)
        {
            if (string.IsNullOrEmpty(Tendangnhap) || string.IsNullOrEmpty(Matkhau) || string.IsNullOrEmpty(MatkhauConfirm))
            {
                TempData["ErrorMessage"] = "Please fill in all fields.";
                return RedirectToAction("Register");
            }
            if (Matkhau != MatkhauConfirm)
            {
                TempData["ErrorMessage"] = "Password and Confirm Password must match.";
                return RedirectToAction("Register");
            }
            var existingAccount = _context.Taikhoans.FirstOrDefault(t => t.Tendangnhap == Tendangnhap);
            if (existingAccount != null)
            {
                TempData["ErrorMessage"] = "Username already exists.";
                return RedirectToAction("Register");
            }

            var taikhoan = new Taikhoan
            {
                Tendangnhap = Tendangnhap,
                Matkhau = Matkhau,
                Chucvu = Chucvu
            };
            _context.Taikhoans.Add(taikhoan);
            _context.SaveChanges();

            return RedirectToAction("RegisterCustomer", new { matk = taikhoan.Matk });
        }

        [HttpGet]
        public IActionResult RegisterCustomer(int matk)
        {
            ViewBag.Matk = matk;
            return View();
        }

        [HttpPost]
        public IActionResult RegisterCustomer(int matk, string Tenkh, string Email, string Sdt, string Diachi)
        {
            if (string.IsNullOrEmpty(Tenkh) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Sdt) || string.IsNullOrEmpty(Diachi))
            {
                TempData["ErrorMessage"] = "Please fill in all fields.";
                return RedirectToAction("RegisterCustomer", new { matk });
            }
            var khachhang = new Khachhang
            {
                Matk = matk,
                Tenkh = Tenkh,
                Email = Email,
                Sdt = Sdt,
                Diachi = Diachi
            };
            _context.Khachhangs.Add(khachhang);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Customer registration successful! Please log in.";
            return RedirectToAction("Login", "Accounts");
        }
    }
}