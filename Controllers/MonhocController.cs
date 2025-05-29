using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSV.Models;
using QLSV.ViewModel;

namespace QLSV.Controllers
{
    [Authorize(AuthenticationSchemes = "Cookies")]
    public class MonhocController : Controller
    {
        private readonly HocbasvContext _context;

        public MonhocController(HocbasvContext context)
        {
            _context = context;
        }


        [Authorize(Roles = "NhapDiem")]
        public async Task<IActionResult> DsLopSinhvien(string maMh, string maNh, string maHk)
        {
            if (string.IsNullOrEmpty(maMh))
                return BadRequest("Thiếu mã môn học");

            ViewBag.MaMh = maMh;
            ViewBag.MaNh = maNh;
            ViewBag.MaHk = maHk;

            var classes = await _context.CtKhhtSvs
                .Where(ct =>
                    ct.MaMh == maMh &&
                    ct.XacNhanDk == true &&
                    (string.IsNullOrEmpty(maNh) || ct.MaNh == maNh) &&    
                    (string.IsNullOrEmpty(maHk) || ct.MaHk == maHk)       
                )
                // GroupBy để distinct theo Lớp + Năm học + Học kỳ
                .GroupBy(ct => new {
                    ct.MaSvNavigation.MaLopNavigation.MaLop,
                    ct.MaSvNavigation.MaLopNavigation.TenLop,
                    ct.MaNh,
                    ct.MaHk
                })
                .Select(g => new LopViewModel
                {
                    MaLop = g.Key.MaLop,
                    TenLop = g.Key.TenLop,
                    MaNh = g.Key.MaNh,
                    NamHoc = _context.Namhocs
                                 .Where(n => n.MaNh == g.Key.MaNh)
                                 .Select(n => n.TenNh)
                                 .FirstOrDefault(),
                    MaHk = g.Key.MaHk,
                    HocKy = _context.Hockies
                                 .Where(h => h.MaHk == g.Key.MaHk)
                                 .Select(h => h.MaHk)
                                 .FirstOrDefault()
                })
                .OrderBy(x => x.MaLop)
                .ToListAsync();

            return View(classes);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> DsSinhvien(string maMh, string maLop, string maNh, string maHk)
        {
            if (string.IsNullOrEmpty(maMh))
                return BadRequest("Thiếu mã môn học");

            ViewBag.MaMh = maMh;
            ViewBag.MaLop = maLop;
            ViewBag.MaNh = maNh;
            ViewBag.MaHk = maHk;

            // 1. Lấy tỷ lệ từ KQHT vào biến locals
            var sample = await _context.Kqhts
                .Where(k => k.MaMh == maMh
                         && k.MaSvNavigation.MaLop == maLop
                         && k.MaNh == maNh
                         && k.MaHk == maHk)
                .Select(k => new { k.TyLeQt, k.TyLeGk, k.TyLeCk })
                .FirstOrDefaultAsync();

            double tyLeQt = sample?.TyLeQt ?? 0;
            double tyLeGk = sample?.TyLeGk ?? 0;
            double tyLeCk = sample?.TyLeCk ?? 0;

            ViewBag.TyLeQt = tyLeQt;
            ViewBag.TyLeGk = tyLeGk;
            ViewBag.TyLeCk = tyLeCk;

            // 2. Query với LEFT JOIN
            var query = from ct in _context.CtKhhtSvs
             .Include(ct => ct.MaSvNavigation)
                        where ct.MaMh == maMh
                              && ct.XacNhanDk == true
                              && ct.MaSvNavigation.MaLop == maLop
                              && (string.IsNullOrEmpty(maNh) || ct.MaNh == maNh)   
                              && (string.IsNullOrEmpty(maHk) || ct.MaHk == maHk)   
                        join kq in _context.Kqhts
                        .Where(k => k.MaMh == maMh
                               && k.MaNh == maNh
                               && k.MaHk == maHk)
                        on new { ct.MaSv, ct.MaMh, ct.MaNh, ct.MaHk }
                        equals new { kq.MaSv, kq.MaMh, kq.MaNh, kq.MaHk }
                        into kqGroup
                        from kq in kqGroup.DefaultIfEmpty() // ← LEFT JOIN
                        orderby ct.MaSvNavigation.HoTen
                        select new NhapDiemViewModel
                        {
                            MaKqht = kq.MaKqht,      
                            MaSv = ct.MaSv,
                            HoTen = ct.MaSvNavigation.HoTen,
                            MaMh = maMh,
                            MaLop = maLop,
                            MaNh = maNh,
                            MaHk = maHk,

                            TyLeQt = tyLeQt,
                            TyLeGk = tyLeGk,
                            TyLeCk = tyLeCk,

                            DiemQt = kq.DiemQt,
                            DiemGk = kq.DiemGk,
                            DiemCk = kq.DiemCk,

                        };


            var list = await query.ToListAsync();
            return View(list);
        }


        /// Lấy tỷ lệ điểm (nếu đã nhập) cho môn–lớp–năm học–học kỳ
        [HttpGet("GetTyLe")]
        public async Task<JsonResult> GetTyLe(string maMh, string maLop, string maNh, string maHk)
        {
            if (new[] { maMh, maLop, maNh, maHk }.Any(string.IsNullOrEmpty))
                return Json(new { success = false, message = "Thiếu tham số." });

            // Join CtKhhtSvs (đã confirm) với Kqhts qua composite key
            var kq = await (
                from ct in _context.CtKhhtSvs
                    .Where(ct =>
                        ct.MaMh == maMh &&
                        ct.XacNhanDk == true && 
                        ct.MaSvNavigation.MaLop == maLop &&
                        ct.MaNh == maNh &&
                        ct.MaHk == maHk)
                join k in _context.Kqhts
                    .Where(k =>
                        k.MaMh == maMh &&
                        k.MaNh == maNh &&
                        k.MaHk == maHk)
                on new { ct.MaSv, ct.MaMh, ct.MaNh, ct.MaHk }
                equals new { k.MaSv, k.MaMh, k.MaNh, k.MaHk }
                select k
            ).FirstOrDefaultAsync();

            return Json(new
            {
                success = true,
                TyLeQT = kq?.TyLeQt ?? 0.0,
                TyLeGK = kq?.TyLeGk ?? 0.0,
                TyLeCK = kq?.TyLeCk ?? 0.0
            });
        }


        [HttpPost("NhapTyLe")]
        public async Task<JsonResult> NhapTyLe([FromBody] TyLeDiemViewModel vm)
        {
            // 1. Validate input
            if (new[] { vm.MaMh, vm.MaLop, vm.MaNh, vm.MaHk }.Any(string.IsNullOrEmpty))
                return Json(new { success = false, message = "Thiếu MaMh/MaLop/MaNh/MaHk." });

            if (vm.TyLeQT + vm.TyLeGK + vm.TyLeCK != 100m)
                return Json(new { success = false, message = $"Tổng tỷ lệ phải = 100%, hiện {(vm.TyLeQT + vm.TyLeGK + vm.TyLeCK)}%." });

            // 2. Lấy tất cả CtKhhtSv đã xác nhận cho tổ hợp (môn–lớp–năm–học kỳ)
            var regs = await _context.CtKhhtSvs
                .Where(ct =>
                    ct.MaMh == vm.MaMh &&
                    ct.XacNhanDk == true &&
                    ct.MaSvNavigation.MaLop == vm.MaLop &&
                    ct.MaNh == vm.MaNh &&
                    ct.MaHk == vm.MaHk)
                .ToListAsync();

            // 3. Lấy existing Kqhts cho tổ hợp này
            var studentIds = regs.Select(r => r.MaSv).ToHashSet();
            var existing = await _context.Kqhts
                .Where(k =>
                    k.MaMh == vm.MaMh &&
                    k.MaNh == vm.MaNh &&
                    k.MaHk == vm.MaHk &&
                    studentIds.Contains(k.MaSv))
                .ToListAsync();

            // 4. Cập nhật hoặc tạo mới
            foreach (var reg in regs)
            {
                var kq = existing.FirstOrDefault(k => k.MaSv == reg.MaSv);
                if (kq == null)
                {
                    kq = new Kqht
                    {
                        MaKqht = Guid.NewGuid().ToString(),
                        MaSv = reg.MaSv,
                        MaMh = vm.MaMh,
                        MaNh = vm.MaNh,
                        MaHk = vm.MaHk,
                        DiemQt = null,
                        DiemGk = null,
                        DiemCk = null
                    };
                    _context.Kqhts.Add(kq);
                    existing.Add(kq);
                }

                // Gán tỷ lệ mới
                kq.TyLeQt = (double)vm.TyLeQT;
                kq.TyLeGk = (double)vm.TyLeGK;
                kq.TyLeCk = (double)vm.TyLeCK;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu tỷ lệ thành công" });
        }

        [HttpPost("[action]")]
        public async Task<JsonResult> SaveDiem([FromForm] List<NhapDiemViewModel> list)
        {
            var errors = new List<string>();

            foreach (var vm in list)
            {
                // 1. Validate điểm nằm trong [0,10]
                if ((vm.DiemQt ?? 0) < 0 || (vm.DiemQt ?? 0) > 10 ||
                    (vm.DiemGk ?? 0) < 0 || (vm.DiemGk ?? 0) > 10 ||
                    (vm.DiemCk ?? 0) < 0 || (vm.DiemCk ?? 0) > 10)
                {
                    errors.Add($"SV {vm.MaSv}: điểm phải trong [0,10].");
                    continue;
                }

                // 2. Tìm record Kqht theo composite key
                var kq = await _context.Kqhts.FirstOrDefaultAsync(k =>
                    k.MaSv == vm.MaSv &&
                    k.MaMh == vm.MaMh &&
                    k.MaNh == vm.MaNh &&
                    k.MaHk == vm.MaHk);

                // 3. Nếu chưa có, tự tạo mới với tỷ lệ đã bind sẵn
                if (kq == null)
                {
                    kq = new Kqht
                    {
                        MaKqht = Guid.NewGuid().ToString(),
                        MaSv = vm.MaSv,
                        MaMh = vm.MaMh,
                        MaNh = vm.MaNh,
                        MaHk = vm.MaHk,
                        TyLeQt = vm.TyLeQt,
                        TyLeGk = vm.TyLeGk,
                        TyLeCk = vm.TyLeCk,
                        // ban đầu điểm null
                        DiemQt = null,
                        DiemGk = null,
                        DiemCk = null
                    };
                    _context.Kqhts.Add(kq);
                }

                // 4. Gán điểm và tính DiemTb
                kq.DiemQt = vm.DiemQt;
                kq.DiemGk = vm.DiemGk;
                kq.DiemCk = vm.DiemCk;
                kq.DiemTb = (
                    (vm.DiemQt ?? 0) * vm.TyLeQt
                  + (vm.DiemGk ?? 0) * vm.TyLeGk
                  + (vm.DiemCk ?? 0) * vm.TyLeCk
                ) / 100.0;

            }

            if (errors.Any())
                return Json(new { success = false, errors });

            // 5. Lưu thay đổi
            var updated = await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message = $"Lưu điểm thành công! (Đã cập nhật {updated} bản ghi)"
            });
        }
        
        // GET: Monhoc
        public async Task<IActionResult> Index(string searchTerm, string sortColumn, string sortOrder, int page = 1, int pageSize = 12)
        {
            //  Tạo query cơ bản, include bộ môn
            var query = _context.Monhocs
                .Include(m => m.MaBmNavigation)
                .AsQueryable();

            //  Tìm kiếm theo Mã MH hoặc Tên MH
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(m =>
                    m.MaMh.Contains(searchTerm) ||
                    m.TenMh.Contains(searchTerm));
            }

            //  Sắp xếp
            switch (sortColumn)
            {
                case "MaMh":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(m => m.MaMh)
                        : query.OrderBy(m => m.MaMh);
                    break;
                case "TenMh":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(m => m.TenMh)
                        : query.OrderBy(m => m.TenMh);
                    break;
                case "SoTc":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(m => m.SoTc)
                        : query.OrderBy(m => m.SoTc);
                    break;
                case "MaBm":
                    query = sortOrder == "desc"
                        ? query.OrderByDescending(m => m.MaBm)
                        : query.OrderBy(m => m.MaBm);
                    break;
                default:
                    // Mặc định sắp xếp theo MaMh
                    query = query.OrderBy(m => m.MaMh);
                    break;
            }

            //  Tổng số bản ghi
            var totalItems = await query.CountAsync();

            //  Phân trang
            var monhocs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //  Đưa về ViewData/ViewBag để xử lý trong View
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;

            return View(monhocs);
        }

        // GET: Monhoc/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monhoc = await _context.Monhocs
                .Include(m => m.MaBmNavigation)
                .FirstOrDefaultAsync(m => m.MaMh == id);
            if (monhoc == null)
            {
                return NotFound();
            }

            return View(monhoc);
        }

        // GET: Monhoc/Create
        [Authorize(Roles = "Them")]
        public IActionResult Create()
        {
            ViewData["MaBm"] = new SelectList(_context.Bomons, "MaBm", "MaBm");
            return View();
        }

        // POST: Monhoc/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaMh,TenMh,SoTc,MaBm")] Monhoc monhoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(monhoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaBm"] = new SelectList(_context.Bomons, "MaBm", "MaBm", monhoc.MaBm);
            return View(monhoc);
        }

        // GET: Monhoc/Edit/5
        [Authorize(Roles = "Sua")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monhoc = await _context.Monhocs.FindAsync(id);
            if (monhoc == null)
            {
                return NotFound();
            }
            ViewData["MaBm"] = new SelectList(_context.Bomons, "MaBm", "MaBm", monhoc.MaBm);
            return View(monhoc);
        }

        // POST: Monhoc/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaMh,TenMh,SoTc,MaBm")] Monhoc monhoc)
        {
            if (id != monhoc.MaMh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(monhoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonhocExists(monhoc.MaMh))
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
            ViewData["MaBm"] = new SelectList(_context.Bomons, "MaBm", "MaBm", monhoc.MaBm);
            return View(monhoc);
        }

        // GET: Monhoc/Delete/5
        [Authorize(Roles = "Xoa")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monhoc = await _context.Monhocs
                .Include(m => m.MaBmNavigation)
                .FirstOrDefaultAsync(m => m.MaMh == id);
            if (monhoc == null)
            {
                return NotFound();
            }

            return View(monhoc);
        }
        // POST: Monhocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // 1. Load môn học cùng tất cả các collection liên quan
            var monhoc = await _context.Monhocs
                .Include(m => m.CtCtdts)      // chi tiết CTĐT
                .Include(m => m.CtKhhts)      // chi tiết KHHT
                .Include(m => m.Kqhts)        // kết quả học tập
                .Include(m => m.MaBmNavigation) // để hiển thị tên Bộ môn nếu cần
                .FirstOrDefaultAsync(m => m.MaMh == id);

            if (monhoc == null)
                return NotFound();

            // 2. Đếm số bản ghi con
            int cntCtCtdt = monhoc.CtCtdts?.Count() ?? 0;
            int cntCtKhht = monhoc.CtKhhts?.Count() ?? 0;
            int cntKqht = monhoc.Kqhts?.Count() ?? 0;

            // 3. Nếu còn liên kết, trả về View kèm thông báo
            if (cntCtCtdt + cntCtKhht + cntKqht > 0)
            {
                ViewBag.DeleteError =
                    $"Không thể xóa môn “{monhoc.TenMh}” vì còn " +
                    $"{cntCtCtdt} chi tiết CTĐT, " +
                    $"{cntCtKhht} chi tiết KHHT và " +
                    $"{cntKqht} kết quả học tập liên quan.";
                return View(monhoc);
            }

            // 4. Nếu không có liên kết, thực hiện xóa
            _context.Monhocs.Remove(monhoc);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MonhocExists(string id)
        {
            return _context.Monhocs.Any(e => e.MaMh == id);
        }
    }
}
