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
        //Feature:Login
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return View("Login");
        }
        [HttpPost]
        public IActionResult Login(string Tendangnhap, string Matkhau)
        {
            var taikhoan = _context.Taikhoans.Where(t => t.Tendangnhap == Tendangnhap && t.Matkhau == Matkhau).FirstOrDefault<Taikhoan>();
            if (taikhoan == null)
            {
                TempData["ErrorMessage"] = "Invalid username or password.";
                return RedirectToAction("Login");
            }
            TempData["SuccessMessage"] = "Login successful! Redirecting...";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, taikhoan.Tendangnhap),
                new Claim(ClaimTypes.Role, taikhoan.Chucvu),
            };
            var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
            return RedirectToAction("Login");
        }
    }
}
