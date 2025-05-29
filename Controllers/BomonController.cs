using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSV.Models;

namespace QLSV.Controllers
{
    [Authorize(AuthenticationSchemes = "Cookies")]
    public class BomonController : Controller
    {
        private readonly HocbasvContext _context;

        public BomonController(HocbasvContext context)
        {
            _context = context;
        }

        // GET: Bomons
        public async Task<IActionResult> Index(string searchTerm, string sortColumn, string sortOrder, int page = 1, int pageSize = 8)
        {
            var query = _context.Bomons.AsQueryable();

            // Tìm kiếm theo tên bộ môn
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.TenBm.Contains(searchTerm));
            }

            // Sắp xếp dữ liệu
            switch (sortColumn)
            {
                case "MaBm":
                    query = sortOrder == "asc" ? query.OrderBy(b => b.MaBm) : query.OrderByDescending(b => b.MaBm);
                    break;
                case "TenBm":
                    query = sortOrder == "asc" ? query.OrderBy(b => b.TenBm) : query.OrderByDescending(b => b.TenBm);
                    break;
                case "MaKhoa":
                    query = sortOrder == "asc" ? query.OrderBy(b => b.MaKhoa) : query.OrderByDescending(b => b.MaKhoa);
                    break;
                default:
                    query = query.OrderBy(b => b.MaBm); // Mặc định sắp xếp theo Mã bộ môn
                    break;
            }

            // Tổng số bản ghi
            int totalItems = await query.CountAsync();

            // Phân trang
            var bomon = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Truyền dữ liệu vào ViewData
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;

            return View(bomon);
        }


        // GET: Bomons/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bomon = await _context.Bomons
                .Include(b => b.MaKhoaNavigation)
                .FirstOrDefaultAsync(m => m.MaBm == id);
            if (bomon == null)
            {
                return NotFound();
            }

            return View(bomon);
        }

        // GET: Bomons/Create
        [Authorize(Roles = "Them")]
        public IActionResult Create()
        {
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa");
            return View();
        }

        // POST: Bomon/Create
        [HttpPost]
        [Authorize(Roles = "Them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaBm,TenBm,MaKhoa")] Bomon bomon)
        {
            // Kiểm tra xem MaBm đã tồn tại chưa
            bool isExist = await _context.Bomons.AnyAsync(b => b.MaBm == bomon.MaBm);
            if (isExist)
            {
                ModelState.AddModelError("MaBm", "Mã bộ môn đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                // Đảm bảo MaKhoaNavigation là null để tránh lỗi khi thêm
                bomon.MaKhoaNavigation = null;

                _context.Add(bomon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, load lại dropdown và hiển thị form
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", bomon.MaKhoa);
            return View(bomon);
        }

        // GET: Bomons/Edit/5
        [Authorize(Roles = "Sua")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bomon = await _context.Bomons.FindAsync(id);
            if (bomon == null)
            {
                return NotFound();
            }
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", bomon.MaKhoa);
            return View(bomon);
        }

        // POST: Bomons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaBm,TenBm,MaKhoa")] Bomon bomon)
        {
            if (id != bomon.MaBm)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bomon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BomonExists(bomon.MaBm))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", bomon.MaKhoa);
            return View(bomon);
        }

        // GET: Bomons/Delete/5
        [Authorize(Roles = "Xoa")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bomon = await _context.Bomons
                .Include(b => b.MaKhoaNavigation)
                .FirstOrDefaultAsync(m => m.MaBm == id);
            if (bomon == null)
            {
                return NotFound();
            }

            return View(bomon);
        }

        // POST: Bomons/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Load cả navigation properties để kiểm tra
            var bomon = await _context.Bomons
                .Include(b => b.Monhocs)    
                .Include(b => b.Giangviens) 
                .FirstOrDefaultAsync(b => b.MaBm == id);

            if (bomon == null)
                return NotFound();

            // Kiểm tra xem có bản ghi con không
            int countMH = bomon.Monhocs?.Count() ?? 0;
            int countGV = bomon.Giangviens?.Count() ?? 0;

            if (countMH + countGV > 0)
            {
                // Đưa thông báo về View
                ViewBag.DeleteError = $"Không thể xóa vì còn {countMH} môn học và {countGV} giảng viên liên quan.";
                return View(bomon);
            }

            // Nếu không có bản ghi con, cho phép xóa
            _context.Bomons.Remove(bomon);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BomonExists(string id)
        {
            return _context.Bomons.Any(e => e.MaBm == id);
        }
    }
}
