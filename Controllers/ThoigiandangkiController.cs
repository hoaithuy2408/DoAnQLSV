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
    [Authorize(Roles = "ADMIN")]
    public class ThoigiandangkiController : Controller
    {
        private readonly HocbasvContext _context;

        public ThoigiandangkiController(HocbasvContext context)
        {
            _context = context;
        }

        // GET: Thoigiandangki
        [Authorize(AuthenticationSchemes = "Cookies")]
        public async Task<IActionResult> Index(string searchTerm, string sortColumn, string sortOrder, int page = 1, int pageSize = 8)
        {
            var query = _context.Thoigiandangkies
                .Include(t => t.MaHkNavigation)
                .Include(t => t.MaNhNavigation)
                .AsQueryable();

            // Tìm kiếm theo năm học hoặc học kỳ
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t =>
                    t.MaNhNavigation.TenNh.Contains(searchTerm) ||
                    t.MaHkNavigation.MaHk.Contains(searchTerm)
                );
            }

            // Sắp xếp theo cột
            switch (sortColumn)
            {
                case "TenNh":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(t => t.MaNhNavigation.TenNh)
                        : query.OrderBy(t => t.MaNhNavigation.TenNh);
                    break;
                case "TenHk":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(t => t.MaHkNavigation.TenHk)
                        : query.OrderBy(t => t.MaHkNavigation.TenHk);
                    break;
                case "NgayBatDau":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(t => t.NgayBatDau)
                        : query.OrderBy(t => t.NgayBatDau);
                    break;
                case "NgayKetThuc":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(t => t.NgayKetThuc)
                        : query.OrderBy(t => t.NgayKetThuc);
                    break;
                default:
                    query = query.OrderBy(t => t.MaNh); // Mặc định sắp xếp theo năm học
                    break;
            }

            // Tổng số bản ghi
            int totalItems = await query.CountAsync();

            // Phân trang
            var result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Truyền dữ liệu vào ViewData/ViewBag
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;

            return View(result);
        }

        // GET: Thoigiandangki/Details?maNh=...&maHk=...
        public async Task<IActionResult> Details(string maNh, string maHk)
        {
            if (maNh == null || maHk == null)
                return NotFound();

            var item = await _context.Thoigiandangkies
                .Include(t => t.MaHkNavigation)
                .Include(t => t.MaNhNavigation)
                .FirstOrDefaultAsync(m => m.MaNh == maNh && m.MaHk == maHk);

            if (item == null)
                return NotFound();

            return View(item);
        }

        public IActionResult Create()
        {
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk");
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNh,MaHk,NgayBatDau,NgayKetThuc,ChoPhepDangKy")] Thoigiandangky model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk", model.MaHk);
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh", model.MaNh);
            return View(model);
        }

        // GET: Thoigiandangki/Edit?maNh=...&maHk=...
        public async Task<IActionResult> Edit(string maNh, string maHk)
        {
            if (maNh == null || maHk == null)
                return NotFound();

            var item = await _context.Thoigiandangkies.FindAsync(maNh, maHk);
            if (item == null)
                return NotFound();

            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk", item.MaHk);
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh", item.MaNh);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string maNh, string maHk,
            [Bind("MaNh,MaHk,NgayBatDau,NgayKetThuc,ChoPhepDangKy")] Thoigiandangky model)
        {
            if (maNh != model.MaNh || maHk != model.MaHk)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThoigiandangkyExists(model.MaNh, model.MaHk))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk", model.MaHk);
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh", model.MaNh);
            return View(model);
        }

        // GET: Thoigiandangki/Delete?maNh=...&maHk=...
        public async Task<IActionResult> Delete(string maNh, string maHk)
        {
            if (maNh == null || maHk == null)
                return NotFound();

            var item = await _context.Thoigiandangkies
                .Include(t => t.MaHkNavigation)
                .Include(t => t.MaNhNavigation)
                .FirstOrDefaultAsync(m => m.MaNh == maNh && m.MaHk == maHk);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string maNh, string maHk)
        {
            var item = await _context.Thoigiandangkies.FindAsync(maNh, maHk);
            if (item != null)
            {
                _context.Thoigiandangkies.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ThoigiandangkyExists(string maNh, string maHk)
        {
            return _context.Thoigiandangkies.Any(e => e.MaNh == maNh && e.MaHk == maHk);
        }
    }
}
