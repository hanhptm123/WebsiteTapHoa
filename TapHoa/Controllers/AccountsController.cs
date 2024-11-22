﻿using System;
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
        //Feature:Login
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Logout()
        {
<<<<<<< HEAD
            // Chỉ xóa thông tin đăng nhập
            HttpContext.Session.Remove("NewCustomerId");
            HttpContext.Session.Remove("NhanvienId");

            // Đăng xuất cookie authentication
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "You have logged out successfully.";
            return RedirectToAction("Login", "Accounts");
        }

=======
            HttpContext.SignOutAsync();
            return View("Login");
        }
>>>>>>> f75a1c2ad8e9004836079902798b6d5c2c78273f
        //[HttpPost]
        //public IActionResult Login(string Tendangnhap, string Matkhau)
        //{
        //    var taikhoan = _context.Taikhoans.Where(t => t.Tendangnhap == Tendangnhap && t.Matkhau == Matkhau).FirstOrDefault<Taikhoan>();
        //    if (taikhoan == null)
        //    {
        //        TempData["ErrorMessage"] = "Invalid username or password.";
        //        return RedirectToAction("Login");
        //    }
        //    TempData["SuccessMessage"] = "Login successful! Redirecting...";
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, taikhoan.Tendangnhap),
        //        new Claim(ClaimTypes.Role, taikhoan.Chucvu),
        //    };
        //    var claimsIdentity = new ClaimsIdentity(
        //    claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //    HttpContext.SignInAsync(
        //    CookieAuthenticationDefaults.AuthenticationScheme,
        //    new ClaimsPrincipal(claimsIdentity));
        //    return RedirectToAction("Login");
        //}
        [HttpPost]
        public IActionResult Login(string Tendangnhap, string Matkhau)
        {
            var taikhoan = _context.Taikhoans.FirstOrDefault(t => t.Tendangnhap == Tendangnhap && t.Matkhau == Matkhau);
            if (taikhoan == null)
            {
                TempData["ErrorMessage"] = "Invalid username or password.";
                return RedirectToAction("Login");
            }

<<<<<<< HEAD
=======
            // Kiểm tra chức vụ
>>>>>>> f75a1c2ad8e9004836079902798b6d5c2c78273f
            if (taikhoan.Chucvu == "Nhanvien")
            {
                var nhanvien = _context.Nhanviens.FirstOrDefault(nv => nv.Matk == taikhoan.Matk);
                if (nhanvien != null)
                {
<<<<<<< HEAD
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
                    TempData["ErrorMessage"] = "Customer account not found.";
                    return RedirectToAction("Login");
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
=======
                    // Lưu mã nhân viên vào session
                    HttpContext.Session.SetInt32("NhanvienId", nhanvien.Manv);
                }
            }
            else
            {
                var khachhang = _context.Khachhangs.FirstOrDefault(kh => kh.Matk == taikhoan.Matk);
                if (khachhang != null)
                {
                    // Lưu mã khách hàng vào session
                    HttpContext.Session.SetInt32("NewCustomerId", khachhang.Makh);
                }
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, taikhoan.Tendangnhap),
                new Claim(ClaimTypes.Role, taikhoan.Chucvu),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
>>>>>>> f75a1c2ad8e9004836079902798b6d5c2c78273f

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

<<<<<<< HEAD
            TempData["SuccessMessage"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }


=======
            // Điều hướng đến trang chính (home)
            return RedirectToAction("Index", "Home");
        }

>>>>>>> f75a1c2ad8e9004836079902798b6d5c2c78273f
        //Feature:Register
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
            TempData["SuccessMessage"] = "Registration successful! Redirecting to login...";
            return RedirectToAction("Login");
        }
    }
}
