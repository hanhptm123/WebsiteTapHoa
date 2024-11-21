using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;

namespace TapHoa.Controllers
{
    public class ReceiptController : Controller
    {
        private readonly TaphoaContext _context;

        public ReceiptController(TaphoaContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var taphoaContext = _context.Phieunhaps.Include(p => p.ManvNavigation);
            return View(await taphoaContext.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["Manv"] = new SelectList(_context.Nhanviens, "Manv", "Manv");
            ViewBag.Products = _context.Sanphams.ToList(); 
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<Ctphieunhap> receiptDetails)
        {
            if (receiptDetails == null || !receiptDetails.Any())
            {
                TempData["Error"] = "The receipt details list cannot be empty.";
                return RedirectToAction("Index");  
            }

            try
            {
                int? employeeId = HttpContext.Session.GetInt32("NhanvienId");
                if (!employeeId.HasValue)
                {
                    TempData["Error"] = "Employee ID is missing. Please log in again.";
                    return RedirectToAction("Login", "Accounts");  
                }

                var newReceipt = new Phieunhap
                {
                    Manv = employeeId.Value,
                    Ngaynhap = DateTime.Now,
                    Tongtien = 0 
                };

                _context.Phieunhaps.Add(newReceipt);
                await _context.SaveChangesAsync();

                int receiptId = newReceipt.Mapn;
                decimal totalAmount = 0;
                List<string> errorMessages = new();

                foreach (var detail in receiptDetails)
                {
                    if (detail.Soluong <= 0 || detail.Gia <= 0)
                    {
                        errorMessages.Add($"Invalid quantity or price for product ID {detail.Masp}.");
                        continue;
                    }

                    var product = await _context.Sanphams.FirstOrDefaultAsync(p => p.Masp == detail.Masp);
                    if (product == null)
                    {
                        errorMessages.Add($"Product with ID {detail.Masp} not found.");
                        continue;
                    }

                    var receiptDetail = new Ctphieunhap
                    {
                        Mapn = receiptId,
                        Masp = detail.Masp,
                        Soluong = detail.Soluong,
                        Gia = detail.Gia,
                        Thanhtien = detail.Soluong * detail.Gia.Value
                    };

                    totalAmount += receiptDetail.Thanhtien;

                    product.Soluong = (product.Soluong ?? 0) + detail.Soluong;

                    _context.Ctphieunhaps.Add(receiptDetail);
                }

                if (errorMessages.Any())
                {
                    TempData["Error"] = string.Join("<br>", errorMessages);
                    return RedirectToAction("Index");
                }

                newReceipt.Tongtien = totalAmount;
                _context.Update(newReceipt);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Receipt created successfully!";
                return RedirectToAction("Index");  
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");  
            }
        }
       
        // GET: Phieunhaps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieunhap = await _context.Phieunhaps
                .Include(p => p.ManvNavigation)
                .FirstOrDefaultAsync(m => m.Mapn == id);
            if (phieunhap == null)
            {
                return NotFound();
            }

            return View(phieunhap);
        }

        // POST: Phieunhaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var phieunhap = await _context.Phieunhaps
                    .Include(p => p.Ctphieunhaps)
                    .ThenInclude(ct => ct.MaspNavigation) 
                    .FirstOrDefaultAsync(p => p.Mapn == id);

                if (phieunhap != null)
                {
                    
                    foreach (var ctPhieunhap in phieunhap.Ctphieunhaps)
                    {
                        var product = ctPhieunhap.MaspNavigation; 
                        if (product != null)
                        {
                            product.Soluong -= ctPhieunhap.Soluong;
                            if (product.Soluong < 0)
                            {
                                product.Soluong = 0;
                            }

                            _context.Sanphams.Update(product); 
                        }
                    }
                    _context.Ctphieunhaps.RemoveRange(phieunhap.Ctphieunhaps);
                    _context.Phieunhaps.Remove(phieunhap);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "The record, its details, and product quantities have been updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Record not found or already deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while deleting: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
        private bool PhieunhapExists(int id)
        {
            return _context.Phieunhaps.Any(e => e.Mapn == id);
        }
    }
}
