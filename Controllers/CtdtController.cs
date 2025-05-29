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
    public class CtdtController : Controller
    {
        private readonly HocbasvContext _context;

        public CtdtController(HocbasvContext context)
        {
            _context = context;
        }

        // GET: Ctdts
        public async Task<IActionResult> Index(string searchTerm, string sortColumn, string sortOrder, int page = 1, int pageSize = 8)
        {
            var query = _context.Ctdts
                .Include(c => c.MaKhoaNavigation)
                .Include(c => c.MaNganhNavigation)

                .AsQueryable();

            // Tìm kiếm theo tên ngành hoặc mã ngành
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.TenCtdt.Contains(searchTerm) || c.MaCtdt.Contains(searchTerm));
            }

            // Sắp xếp dữ liệu
            switch (sortColumn)
            {
                case "MaCtdt":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.MaCtdt) : query.OrderByDescending(c => c.MaCtdt);
                    break;
                case "MaNganh":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.MaNganh) : query.OrderByDescending(c => c.MaNganh);
                    break;
                case "MaKhoa":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.MaKhoa) : query.OrderByDescending(c => c.MaKhoa);
                    break;
                
                default:
                    query = query.OrderBy(c => c.MaCtdt); // Mặc định sắp xếp theo Mã CTĐT
                    break;
            }

            // Tổng số bản ghi
            int totalItems = await query.CountAsync();

            // Phân trang
            var ctdts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Truyền dữ liệu vào ViewData
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;

            return View(ctdts);
        }

        // GET: Ctdts/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ctdt = await _context.Ctdts
                .Include(c => c.MaKhoaNavigation)
                .Include(c => c.MaNganhNavigation)
                .FirstOrDefaultAsync(m => m.MaCtdt == id);
            if (ctdt == null)
            {
                return NotFound();
            }

            return View(ctdt);
        }

        // GET: Ctdts/Create
        [Authorize(Roles = "Them")]
        public IActionResult Create()
        {
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa");
            ViewData["MaNganh"] = new SelectList(_context.Nganhs, "MaNganh", "TenNganh");
            return View();
        }

        // POST: Ctdts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCtdt,TenCtdt,HinhThucDt,TongSoTc,MaKhoa,MaNganh")] Ctdt ctdt)
        {

            bool isExist = await _context.Ctdts.AnyAsync(c => c.MaCtdt == ctdt.MaCtdt);
            if (isExist)
            {
                ModelState.AddModelError("MaCtdt", "Mã CTĐT đã tồn tại.");
            }
            if (ModelState.IsValid)
            {
                ctdt.MaKhoaNavigation = null;
                ctdt.MaNganhNavigation = null;

                _context.Add(ctdt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", ctdt.MaKhoa);
            ViewData["MaNganh"] = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", ctdt.MaNganh);
            return View(ctdt);

            
        }

        // GET: Ctdts/Edit/5
        [Authorize(Roles = "Sua")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ctdt = await _context.Ctdts.FindAsync(id);
            if (ctdt == null)
            {
                return NotFound();
            }
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", ctdt.MaKhoa);
            ViewData["MaNganh"] = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", ctdt.MaNganh);
            return View(ctdt);
        }

        // POST: Ctdts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaCtdt,TenCtdt,HinhThucDt,TongSoTc,MaKhoa,MaNganh")] Ctdt ctdt)
        {
            if (id != ctdt.MaCtdt)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ctdt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CtdtExists(ctdt.MaCtdt))
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
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", ctdt.MaKhoa);
            ViewData["MaNganh"] = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", ctdt.MaNganh);
            return View(ctdt);
        }

        // GET: Ctdts/Delete/5
        [Authorize(Roles = "Xoa")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ctdt = await _context.Ctdts
                .Include(c => c.MaKhoaNavigation)
                .Include(c => c.MaNganhNavigation)
                .FirstOrDefaultAsync(m => m.MaCtdt == id);
            if (ctdt == null)
            {
                return NotFound();
            }

            return View(ctdt);
        }

        // POST: Ctdts/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // 1. Load Ctdt cùng tất cả các collection liên quan
            var ctdt = await _context.Ctdts
                .Include(c => c.MaKhoaNavigation)       
                .Include(c => c.CtCtdts)          
                .Include(c => c.Khhts)           
                .Include(c => c.Sinhviens)        
                .Include(c => c.XettotnghiepNhoms)
                .FirstOrDefaultAsync(c => c.MaCtdt == id);

            if (ctdt == null)
                return NotFound();

            // 2. Đếm số bản ghi con
            int countChiTiet = ctdt.CtCtdts?.Count() ?? 0;
            int countKhht = ctdt.Khhts?.Count() ?? 0;
            int countSv = ctdt.Sinhviens?.Count() ?? 0;
            int countXetTotNgh = ctdt.XettotnghiepNhoms?.Count() ?? 0;

            // 3. Nếu còn liên kết, trả view với thông báo
            if (countChiTiet + countKhht + countSv + countXetTotNgh > 0)
            {
                ViewBag.DeleteError =
                    $"Không thể xóa CTĐT vì còn " +
                    $"{countChiTiet} chi tiết CTĐT, " +
                    $"{countKhht} KHHT, " +
                    $"{countSv} sinh viên và " +
                    $"{countXetTotNgh} nhóm xét tốt nghiệp liên quan.";
                return View(ctdt);
            }
            if (ViewBag.DeleteError != null)
            {
                // trả lại view với đầy đủ dữ liệu
                return View(ctdt);
            }
            // 4. Ngược lại: xóa và chuyển về Index
            _context.Ctdts.Remove(ctdt);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CtdtExists(string id)
        {
            return _context.Ctdts.Any(e => e.MaCtdt == id);
        }
    }
}
