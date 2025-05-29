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
    public class KhhtController : Controller
    {
        private readonly HocbasvContext _context;

        public KhhtController(HocbasvContext context)
        {
            _context = context;
        }

        // GET: Khht
        public async Task<IActionResult> Index(string searchTerm, string sortColumn, string sortOrder, int page = 1, int pageSize = 8)
        {
            var query = _context.Khhts
                .Include(k => k.MaCtdtNavigation)
                .Include(k => k.MaHkNavigation)
                .Include(k => k.MaNhNavigation)
                    .Include(k => k.MaNkNavigation)

                .AsQueryable();

            // Tìm kiếm theo tên hoặc mã KHHT
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(k => k.TenKhht.Contains(searchTerm) || k.MaKhht.Contains(searchTerm));
            }

            // Sắp xếp dữ liệu
            switch (sortColumn)
            {
                case "MaKhht":
                    query = sortOrder == "asc" ? query.OrderBy(k => k.MaKhht) : query.OrderByDescending(k => k.MaKhht);
                    break;
                case "TenKhht":
                    query = sortOrder == "asc" ? query.OrderBy(k => k.TenKhht) : query.OrderByDescending(k => k.TenKhht);
                    break;
                case "MaCtdt":
                    query = sortOrder == "asc" ? query.OrderBy(k => k.MaCtdt) : query.OrderByDescending(k => k.MaCtdt);
                    break;
                case "MaNh":
                    query = sortOrder == "asc" ? query.OrderBy(k => k.MaNh) : query.OrderByDescending(k => k.MaNh);
                    break;
                case "MaHk":
                    query = sortOrder == "asc" ? query.OrderBy(k => k.MaHk) : query.OrderByDescending(k => k.MaHk);
                    break;
                default:
                    query = query.OrderBy(k => k.MaKhht); // Mặc định sắp xếp theo Mã KHHT
                    break;
            }

            // Tổng số bản ghi
            int totalItems = await query.CountAsync();

            // Phân trang
            var khhts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Truyền dữ liệu vào ViewData
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;

            return View(khhts);
        }

        // GET: Khht/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khht = await _context.Khhts
                .Include(k => k.MaCtdtNavigation)
                .Include(k => k.MaHkNavigation)
                .Include(k => k.MaNhNavigation)
                .Include(k => k.MaNkNavigation)
                .FirstOrDefaultAsync(m => m.MaKhht == id);
            if (khht == null)
            {
                return NotFound();
            }

            return View(khht);
        }

        // GET: Khht/Create
        [Authorize(Roles = "Them")]
        public IActionResult Create()
        {
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "TenCtdt");
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk");
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh");
            ViewData["MaNk"] = new SelectList(_context.Nienkhoas, "MaNk", "TenNk");
            return View();
        }


        // POST: Khht/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKhht,TenKhht,MaCtdt,MaNk,MaNh,MaHk,SoTctong,SoTcbatBuoc,SoTctuChon")] Khht khht)
        {
            bool isExist = await _context.Khhts.AnyAsync(k => k.MaKhht == khht.MaKhht);
            if (isExist)
            {
                ModelState.AddModelError("MaKhht", "Mã KHHT đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                khht.MaCtdtNavigation = null;
                khht.MaHkNavigation = null;
                khht.MaNhNavigation = null;
                khht.MaNkNavigation = null;

                _context.Add(khht);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại danh sách với dữ liệu được chọn trước đó
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "TenCtdt", khht.MaCtdt);
            ViewData["MaNk"] = new SelectList(_context.Nienkhoas, "MaNk", "TenNk", khht.MaNk);
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh", khht.MaNh);
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk", khht.MaHk);

            return View(khht);
        }

        // GET: Khht/Edit/5
        [Authorize(Roles = "Sua")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khht = await _context.Khhts.FindAsync(id);
            if (khht == null)
            {
                return NotFound();
            }

            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "TenCtdt", khht.MaCtdt);
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk", khht.MaHk);
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh", khht.MaNh);
            ViewData["MaNk"] = new SelectList(_context.Nienkhoas, "MaNk", "TenNk", khht.MaNk);

            return View(khht);
        }

        // POST: Khht/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaKhht,TenKhht,SoTctong,SoTctuChon,SoTcbatBuoc,MaNh,MaHk,MaCtdt,MaNk")] Khht khht)
        {
            if (id != khht.MaKhht)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khht);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhhtExists(khht.MaKhht))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Load lại SelectList nếu có lỗi
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "TenCtdt", khht.MaCtdt);
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk", khht.MaHk);
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh", khht.MaNh);
            ViewData["MaNk"] = new SelectList(_context.Nienkhoas, "MaNk", "TenNk", khht.MaNk);

            return View(khht);
        }

        // GET: Khht/Delete/5
        [Authorize(Roles = "Xoa")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khht = await _context.Khhts
                .Include(k => k.MaCtdtNavigation)
                .Include(k => k.MaHkNavigation)
                .Include(k => k.MaNhNavigation)
                .Include(k => k.MaNkNavigation)
                .FirstOrDefaultAsync(m => m.MaKhht == id);
            if (khht == null)
            {
                return NotFound();
            }

            return View(khht);
        }

        // POST: Khht/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var khht = await _context.Khhts.FindAsync(id);
            if (khht != null)
            {
                _context.Khhts.Remove(khht);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KhhtExists(string id)
        {
            return _context.Khhts.Any(e => e.MaKhht == id);
        }
    }
}
