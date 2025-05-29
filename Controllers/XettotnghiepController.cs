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
using QLSV.ViewModels;

namespace QLSV.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class XettotnghiepController : Controller
    {
        private readonly HocbasvContext _context;

        public XettotnghiepController(HocbasvContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> DanhSachMonTheoNhom(string maCtdt)
        {
            if (string.IsNullOrEmpty(maCtdt))
                return BadRequest("Thiếu mã chương trình đào tạo.");

            // 1. Lấy tất cả các nhóm học phần đã đăng ký cho chương trình này
            var xetNhom = await _context.XettotnghiepNhoms
                .Include(x => x.MaNhomHpNavigation)
                .Where(x => x.MaCtdt == maCtdt)
                .ToListAsync();

            // 2. Với mỗi nhóm, lấy các môn học từ bảng CtCtdt
            var result = new List<MonTheoNhomViewModel>();

            foreach (var xn in xetNhom)
            {
                var monHocs = await _context.CtCtdts
                    .Include(ct => ct.MaMhNavigation)
                    .Where(ct => ct.MaCtdt == maCtdt
                              && ct.MaNhomHp == xn.MaNhomHp)
                    .Select(ct => new MonHocViewModel
                    {
                        MaMh = ct.MaMh,
                        TenMh = ct.MaMhNavigation.TenMh,
                        SoTc = ct.MaMhNavigation.SoTc
                    })
                    .ToListAsync();

                result.Add(new MonTheoNhomViewModel
                {
                    MaCtdt = xn.MaCtdt,
                    MaNhomHp = xn.MaNhomHp,
                    TenNhomHp = xn.MaNhomHpNavigation.TenNhomHp,
                    MonHocs = monHocs
                });
            }
            ViewBag.MaCtdt = maCtdt;

            // 3. Trả về View (hoặc Json tuỳ bạn)
            return View(result);
        }
        [HttpGet]
        public async Task<IActionResult> ThemNhomHocPhan(string maCtdt)
        {
            if (string.IsNullOrEmpty(maCtdt))
                return BadRequest("Thiếu mã CTĐT.");

            // Lấy list nhóm HP chưa nằm trong XettotnghiepNhom cho CTĐT này
            var daCo = await _context.XettotnghiepNhoms
                .Where(x => x.MaCtdt == maCtdt)
                .Select(x => x.MaNhomHp)
                .ToListAsync();

            var available = await _context.Nhomhps
                .Where(n => !daCo.Contains(n.MaNhomHp))
                .Select(n => new SelectListItem
                {
                    Value = n.MaNhomHp,
                    Text = $"{n.TenNhomHp} ({n.MaNhomHp})"
                })
                .ToListAsync();

            var vm = new AddNhomHocPhanViewModel
            {
                MaCtdt = maCtdt,
                NhomHpList = available
            };
            return View(vm);
        }

        // POST: Xử lý thêm nhóm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemNhomHocPhan(AddNhomHocPhanViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // reload dropdown
                var daCo = await _context.XettotnghiepNhoms
                    .Where(x => x.MaCtdt == vm.MaCtdt)
                    .Select(x => x.MaNhomHp)
                    .ToListAsync();
                vm.NhomHpList = await _context.Nhomhps
                    .Where(n => !daCo.Contains(n.MaNhomHp))
                    .Select(n => new SelectListItem
                    {
                        Value = n.MaNhomHp,
                        Text = $"{n.TenNhomHp} ({n.MaNhomHp})"
                    })
                    .ToListAsync();

                return View(vm);
            }

            // Tạo bản ghi mới
            var newEntry = new XettotnghiepNhom
            {
                MaCtdt = vm.MaCtdt,
                MaNhomHp = vm.MaNhomHp,
                SoTcTuChon = vm.SoTcTuChon,
                SoTcBatBuoc = vm.SoTcBatBuoc
            };
            _context.XettotnghiepNhoms.Add(newEntry);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm nhóm học phần thành công.";
            return RedirectToAction(nameof(DanhSachMonTheoNhom), new { maCtdt = vm.MaCtdt });
        }

        // GET: Xettotnghiep
        [Authorize(AuthenticationSchemes = "Cookies")]
        public async Task<IActionResult> Index()
        {
            var hocbasvContext = _context.Xettotnghieps.Include(x => x.MaCtdtNavigation);
            return View(await hocbasvContext.ToListAsync());
        }

        // GET: Xettotnghiep/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xettotnghiep = await _context.Xettotnghieps
                .Include(x => x.MaCtdtNavigation)
                .FirstOrDefaultAsync(m => m.MaCtdt == id);
            if (xettotnghiep == null)
            {
                return NotFound();
            }

            return View(xettotnghiep);
        }

        // GET: Xettotnghiep/Create
        public IActionResult Create()
        {
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "MaCtdt");
            return View();
        }

        // POST: Xettotnghiep/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCtdt,SoTcTong,SoTcTuChon,SoTcBatBuoc,GpatoiThieu")] Xettotnghiep xettotnghiep)
        {
            if (ModelState.IsValid)
            {
                _context.Add(xettotnghiep);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "MaCtdt", xettotnghiep.MaCtdt);
            return View(xettotnghiep);
        }

        // GET: Xettotnghiep/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xettotnghiep = await _context.Xettotnghieps.FindAsync(id);
            if (xettotnghiep == null)
            {
                return NotFound();
            }
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "MaCtdt", xettotnghiep.MaCtdt);
            return View(xettotnghiep);
        }

        // POST: Xettotnghiep/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaCtdt,SoTcTong,SoTcTuChon,SoTcBatBuoc,GpatoiThieu")] Xettotnghiep xettotnghiep)
        {
            if (id != xettotnghiep.MaCtdt)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(xettotnghiep);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!XettotnghiepExists(xettotnghiep.MaCtdt))
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
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "MaCtdt", xettotnghiep.MaCtdt);
            return View(xettotnghiep);
        }

        // GET: Xettotnghiep/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xettotnghiep = await _context.Xettotnghieps
                .Include(x => x.MaCtdtNavigation)
                .FirstOrDefaultAsync(m => m.MaCtdt == id);
            if (xettotnghiep == null)
            {
                return NotFound();
            }

            return View(xettotnghiep);
        }

        // POST: Xettotnghiep/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var xettotnghiep = await _context.Xettotnghieps.FindAsync(id);
            if (xettotnghiep != null)
            {
                _context.Xettotnghieps.Remove(xettotnghiep);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool XettotnghiepExists(string id)
        {
            return _context.Xettotnghieps.Any(e => e.MaCtdt == id);
        }
    }
}
