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
    public class KhoaController : Controller
    {
        private readonly HocbasvContext _context;

        public KhoaController(HocbasvContext context)
        {
            _context = context;
        }

        // GET: Khoa
        public async Task<IActionResult> Index(string searchTerm, string sortColumn, string sortOrder, int page = 1, int pageSize = 8)
        {
            var query = _context.Khoas.AsQueryable();

            // Tìm kiếm theo tên khoa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(k => k.TenKhoa.Contains(searchTerm)||k.MaKhoa.Contains(searchTerm));
            }

            // Sắp xếp dữ liệu
            switch (sortColumn)
            {
                case "MaKhoa":
                    query = sortOrder == "asc" ? query.OrderBy(k => k.MaKhoa) : query.OrderByDescending(k => k.MaKhoa);
                    break;
                case "TenKhoa":
                    query = sortOrder == "asc" ? query.OrderBy(k => k.TenKhoa) : query.OrderByDescending(k => k.TenKhoa);
                    break;
                default:
                    query = query.OrderBy(k => k.MaKhoa); // Mặc định sắp xếp theo mã khoa
                    break;
            }

            // Tổng số bản ghi
            int totalItems = await query.CountAsync();

            // Phân trang
            var khoas = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Truyền dữ liệu vào ViewData
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;

            return View(khoas);
        }


        // GET: Khoa/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khoa = await _context.Khoas
                .FirstOrDefaultAsync(m => m.MaKhoa == id);
            if (khoa == null)
            {
                return NotFound();
            }

            return View(khoa);
        }

        // GET: Khoa/Create
        [Authorize(Roles = "Them")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Khoa/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKhoa,TenKhoa")] Khoa khoa)
        {
        
            // Kiểm tra xem MaKhoa đã tồn tại chưa
            bool isExist = await _context.Khoas.AnyAsync(a => a.MaKhoa == khoa.MaKhoa);
            if (isExist)
            {
                ModelState.AddModelError("MaKhoa", "Mã khoa đã tồn tại.");
            }

            
            if (ModelState.IsValid)
            {
                _context.Add(khoa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(khoa);
        }

        // GET: Khoa/Edit/5
        [Authorize(Roles = "Sua")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa == null)
            {
                return NotFound();
            }
            return View(khoa);
        }

        // POST: Khoa/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaKhoa,TenKhoa")] Khoa khoa)
        {
            if (id != khoa.MaKhoa)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khoa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhoaExists(khoa.MaKhoa))
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
            return View(khoa);
        }

        // GET: Khoa/Delete/5
        [Authorize(Roles = "Xoa")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khoa = await _context.Khoas
                .FirstOrDefaultAsync(m => m.MaKhoa == id);
            if (khoa == null)
            {
                return NotFound();
            }

            return View(khoa);
        }

        // POST: Khoas/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // 1. Load cả navigation properties để kiểm tra
            var khoa = await _context.Khoas
                .Include(k => k.Bomons)      // bộ môn
                .Include(k => k.Ctdts)       // chương trình đào tạo
                .Include(k => k.Lops)        // lớp
                .Include(k => k.Nganhs)      // ngành
                .Include(k => k.Sinhviens)   // sinh viên
                .FirstOrDefaultAsync(k => k.MaKhoa == id);

            if (khoa == null)
                return NotFound();

            // 2. Đếm số bản ghi con
            int countBm = khoa.Bomons?.Count() ?? 0;
            int countCtdt = khoa.Ctdts?.Count() ?? 0;
            int countLop = khoa.Lops?.Count() ?? 0;
            int countNgh = khoa.Nganhs?.Count() ?? 0;
            int countSv = khoa.Sinhviens?.Count() ?? 0;

            // 3. Nếu còn bản ghi con, trả về View với thông báo
            if (countBm + countCtdt + countLop + countNgh + countSv > 0)
            {
                ViewBag.DeleteError =
                    $"Không thể xóa khoa này vì còn " +
                    $"{countBm} bộ môn, " +
                    $"{countCtdt} chương trình đào tạo, " +
                    $"{countLop} lớp, " +
                    $"{countNgh} ngành và " +
                    $"{countSv} sinh viên liên quan.";
                return View(khoa);
            }

            // 4. Nếu không có bản ghi con, thực hiện xóa
            _context.Khoas.Remove(khoa);
            await _context.SaveChangesAsync();

            // 5. Redirect về danh sách Index
            return RedirectToAction(nameof(Index));
        }

        private bool KhoaExists(string id)
        {
            return _context.Khoas.Any(e => e.MaKhoa == id);
        }
    }
}
