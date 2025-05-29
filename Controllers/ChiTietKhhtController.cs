using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSV.Models;

namespace QLSV.Controllers
{
    public class ChiTietKhhtController : Controller
    {
        private readonly HocbasvContext _context;

        public ChiTietKhhtController(HocbasvContext context)
        {
            _context = context;
        }

        public IActionResult LoadChiTiet(string maKhht)
        {
            if (string.IsNullOrEmpty(maKhht))
                return BadRequest("Mã KHHT không hợp lệ.");

            var khht = _context.Khhts
                .Include(x => x.MaNhNavigation)
                .Include(x => x.MaHkNavigation)
                .FirstOrDefault(x => x.MaKhht == maKhht);

            if (khht == null)
                return NotFound("Không tìm thấy KHHT.");

            // Lấy danh sách môn học từ CTDT (theo MaCTDT + học kỳ + năm học)
            var dsTuCtdt = _context.CtCtdts
                .Where(x => x.MaCtdt == khht.MaCtdt && x.MaHk == khht.MaHk && x.MaNh == khht.MaNh)
                .ToList();

            var maMhTuCtdt = dsTuCtdt.Select(x => x.MaMh).ToHashSet();

            // Lấy danh sách môn học hiện có trong CT_KHHT
            var ctKhhtHienCo = _context.CtKhhts
                .Where(x => x.MaKhht == maKhht)
                .ToList();

            var maMhDaCo = ctKhhtHienCo.Select(x => x.MaMh).ToHashSet();

            // ==== 1. XÓA những môn KHÔNG còn trong CTDT ====
            var ctKhhtCanXoa = ctKhhtHienCo
                .Where(x => !maMhTuCtdt.Contains(x.MaMh))
                .ToList();

            if (ctKhhtCanXoa.Any())
            {
                // Trước tiên, xóa các bản ghi liên quan trong CT_KHHT_SV
                var maMhCanXoa = ctKhhtCanXoa.Select(x => x.MaMh).ToList();

                var ctKhhtSvCanXoa = _context.CtKhhtSvs
                    .Where(x => x.MaKhht == maKhht && maMhCanXoa.Contains(x.MaMh))
                    .ToList();

                if (ctKhhtSvCanXoa.Any())
                {
                    _context.CtKhhtSvs.RemoveRange(ctKhhtSvCanXoa);
                }

                _context.CtKhhts.RemoveRange(ctKhhtCanXoa);
                _context.SaveChanges();
            }

            // ==== 2. THÊM những môn MỚI trong CTDT nhưng chưa có trong CT_KHHT ====
            var monMoi = new List<CtKhht>();

            foreach (var ct in dsTuCtdt)
            {
                if (!maMhDaCo.Contains(ct.MaMh))
                {
                    var mh = _context.Monhocs.FirstOrDefault(x => x.MaMh == ct.MaMh);
                    if (mh != null)
                    {
                        monMoi.Add(new CtKhht
                        {
                            MaKhht = maKhht,
                            MaMh = ct.MaMh,
                            LoaiMh = ct.LoaiMh,
                            MaHk = khht.MaHk,
                            MaNh = khht.MaNh,
                            MaMhNavigation = mh,
                            MaNhNavigation = khht.MaNhNavigation,
                            MaHkNavigation = khht.MaHkNavigation
                        });
                    }
                }
            }

            if (monMoi.Any())
            {
                _context.CtKhhts.AddRange(monMoi);
                _context.SaveChanges();
            }

            // ==== 3. LOAD lại danh sách môn học để hiển thị ====
            var dsMonHoc = _context.CtKhhts
                .Include(x => x.MaMhNavigation)
                .Include(x => x.MaNhNavigation)
                .Include(x => x.MaHkNavigation)
                .Where(x => x.MaKhht == maKhht)
                .ToList();

            return PartialView("_ChiTietKHHT", dsMonHoc);
        }

        // GET: ChiTietKhht
        public async Task<IActionResult> Index(
            string searchTerm,
            string sortColumn,
            string sortOrder,
            int page = 1,
            int pageSize = 8)
        {
            // 1. Khởi tạo IQueryable với các Include cần thiết
            var query = _context.CtKhhts
                .Include(c => c.MaKhhtNavigation)
                .Include(c => c.MaMhNavigation)
                .AsQueryable();

            // 2. Filter theo searchTerm trên mã KHHT hoặc mã MH
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    c.MaKhhtNavigation.MaKhht.Contains(searchTerm) ||
                    c.MaMhNavigation.MaMh.Contains(searchTerm));
            }

            // 3. Sort theo cột được chọn
            //    Nếu sortOrder không truyền vào, mặc định "asc"
            sortOrder = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
                ? "desc"
                : "asc";

            switch (sortColumn)
            {
                case "MaKhht":
                    query = sortOrder == "asc"
                        ? query.OrderBy(c => c.MaKhhtNavigation.MaKhht)
                        : query.OrderByDescending(c => c.MaKhhtNavigation.MaKhht);
                    break;

                case "MaMh":
                    query = sortOrder == "asc"
                        ? query.OrderBy(c => c.MaMhNavigation.MaMh)
                        : query.OrderByDescending(c => c.MaMhNavigation.MaMh);
                    break;

                default:
                    // Mặc định sort theo MaKhht ascending
                    query = query.OrderBy(c => c.MaKhhtNavigation.MaKhht);
                    break;
            }

            // 4. Tính tổng số bản ghi để phân trang
            int totalItems = await query.CountAsync();

            // 5. Lấy dữ liệu cho trang hiện tại
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 6. Đưa về ViewData/ViewBag để View có thể build UI
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.PageSize = pageSize;

            return View(items);
        }

        // GET: ChiTietKhht/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ctKhht = await _context.CtKhhts
                .Include(c => c.MaKhhtNavigation)
                .Include(c => c.MaMhNavigation)
                .FirstOrDefaultAsync(m => m.MaKhht == id);
            if (ctKhht == null)
            {
                return NotFound();
            }

            return View(ctKhht);
        }

        // GET: ChiTietKhht/Create
        public IActionResult Create()
        {
            ViewData["MaKhht"] = new SelectList(_context.Khhts, "MaKhht", "MaKhht");
            ViewData["MaMh"] = new SelectList(_context.Monhocs, "MaMh", "MaMh");
            return View();
        }

        // POST: ChiTietKhht/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKhht,MaMh,LoaiMh,XacNhanDk")] CtKhht ctKhht)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ctKhht);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKhht"] = new SelectList(_context.Khhts, "MaKhht", "MaKhht", ctKhht.MaKhht);
            ViewData["MaMh"] = new SelectList(_context.Monhocs, "MaMh", "MaMh", ctKhht.MaMh);
            return View(ctKhht);
        }

        // GET: ChiTietKhht/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ctKhht = await _context.CtKhhts.FindAsync(id);
            if (ctKhht == null)
            {
                return NotFound();
            }
            ViewData["MaKhht"] = new SelectList(_context.Khhts, "MaKhht", "MaKhht", ctKhht.MaKhht);
            ViewData["MaMh"] = new SelectList(_context.Monhocs, "MaMh", "MaMh", ctKhht.MaMh);
            return View(ctKhht);
        }

        // POST: ChiTietKhht/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaKhht,MaMh,LoaiMh,XacNhanDk")] CtKhht ctKhht)
        {
            if (id != ctKhht.MaKhht)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ctKhht);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CtKhhtExists(ctKhht.MaKhht))
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
            ViewData["MaKhht"] = new SelectList(_context.Khhts, "MaKhht", "MaKhht", ctKhht.MaKhht);
            ViewData["MaMh"] = new SelectList(_context.Monhocs, "MaMh", "MaMh", ctKhht.MaMh);
            return View(ctKhht);
        }

        // GET: CtKhht/Delete?maKhht=...&maMh=...
        [HttpGet]
        public async Task<IActionResult> Delete(string maKhht, string maMh)
        {
            if (string.IsNullOrEmpty(maKhht) || string.IsNullOrEmpty(maMh))
                return BadRequest();

            var item = await _context.CtKhhts
                .FirstOrDefaultAsync(c =>
                    c.MaKhht == maKhht &&
                    c.MaMh == maMh);

            if (item == null)
                return NotFound();

            return View(item);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string maKhht, string maMh, string activeTab = "#general")
        {
            if (string.IsNullOrEmpty(maKhht) || string.IsNullOrEmpty(maMh))
            {
                ViewBag.DeleteError = $"maKhht: {maKhht}, maMh: {maMh} --- Không nhận được dữ liệu!";
                return View();
            }

            var children = _context.CtKhhtSvs
                .Where(sv => sv.MaKhht == maKhht && sv.MaMh == maMh)
                .ToList();

            var parent = await _context.CtKhhts
                .FirstOrDefaultAsync(c => c.MaKhht == maKhht && c.MaMh == maMh);

            if (parent == null)
            {
                ViewBag.DeleteError = "Không tìm thấy chi tiết KHHT cần xóa!";
                return View();
            }

            if (children.Any())
                _context.CtKhhtSvs.RemoveRange(children);

            _context.CtKhhts.Remove(parent);

            await _context.SaveChangesAsync();

            // Kiểm tra lại
            var checkParent = await _context.CtKhhts
                .FirstOrDefaultAsync(c => c.MaKhht == maKhht && c.MaMh == maMh);

            if (checkParent != null)
            {
                ViewBag.DeleteError = "Xóa KHÔNG thành công, bản ghi vẫn còn trong DB!";
                return View();
            }

            return RedirectToAction("Details", "Khht", new { id = maKhht, tab = activeTab.TrimStart('#') });
        }
        private bool CtKhhtExists(string id)
        {
            return _context.CtKhhts.Any(e => e.MaKhht == id);
        }
    }
}
