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
    public class XettotnghiepNhomController : Controller
    {
        private readonly HocbasvContext _context;

        public XettotnghiepNhomController(HocbasvContext context)
        {
            _context = context;
        }

       
        // GET: XettotnghiepNhom
        public async Task<IActionResult> Index()
        {
            var hocbasvContext = _context.XettotnghiepNhoms.Include(x => x.MaCtdtNavigation).Include(x => x.MaNhomHpNavigation);
            return View(await hocbasvContext.ToListAsync());
        }

        // GET: XettotnghiepNhom/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var xettotnghiepNhom = await _context.XettotnghiepNhoms
                .Include(x => x.MaCtdtNavigation)
                .Include(x => x.MaNhomHpNavigation)
                .FirstOrDefaultAsync(m => m.MaCtdt == id);
            if (xettotnghiepNhom == null)
            {
                return NotFound();
            }

            return View(xettotnghiepNhom);
        }

        // GET: XettotnghiepNhom/Create
        public IActionResult Create()
        {
            // Dropdown CTĐT
            ViewBag.AllCtdt = new SelectList(
                _context.Ctdts.Select(c => new {
                    c.MaCtdt,
                    Display = c.MaCtdt + " - " + c.TenCtdt
                }),
                "MaCtdt", "Display"
            );

            // Dropdown nhóm HP
            ViewBag.AllNhomHp = new SelectList(
                _context.Nhomhps.Select(n => new {
                    n.MaNhomHp,
                    Display = n.MaNhomHp + " - " + n.TenNhomHp
                }),
                "MaNhomHp", "Display"
            );

            // Khởi tạo model trống
            return View(new XettotnghiepNhom());
        }

        // POST: XettotnghiepNhom/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCtdt,MaNhomHp,SoTcTuChon,SoTcBatBuoc")] XettotnghiepNhom model)
        {
            if (!ModelState.IsValid)
            {
                // Phải reload dropdown khi trả view lại
                ViewBag.AllCtdt = new SelectList(_context.Ctdts, "MaCtdt", "MaCtdt", model.MaCtdt);
                ViewBag.AllNhomHp = new SelectList(_context.Nhomhps, "MaNhomHp", "MaNhomHp", model.MaNhomHp);
                return View(model);
            }

            _context.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { maCtdt = model.MaCtdt });
        }

        // GET: XetTotNghiepNhom/Edit?maCtdt=CT02-63&maNhomHp=KTPM
        [HttpGet]
        public async Task<IActionResult> Edit(string maCtdt, string maNhomHp)
        {
            if (string.IsNullOrEmpty(maCtdt) || string.IsNullOrEmpty(maNhomHp))
                return BadRequest("Thiếu khóa chỉnh sửa.");

            var entity = await _context.XettotnghiepNhoms
                .Include(x => x.MaCtdtNavigation)
                .Include(x => x.MaNhomHpNavigation)
                .FirstOrDefaultAsync(x => x.MaCtdt == maCtdt && x.MaNhomHp == maNhomHp);

            if (entity == null)
                return NotFound("Không tìm thấy cấu hình nhóm này.");

            // Đưa lên ViewBag để hiển thị readonly hoặc dropdown nếu cần
            ViewBag.MaCtdt = entity.MaCtdt;
            ViewBag.MaNhomHp = entity.MaNhomHp;
            ViewBag.TenNhomHp = entity.MaNhomHpNavigation.TenNhomHp;

            return View(entity);
        }

        // POST: XetTotNghiepNhom/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string maCtdt, string maNhomHp, [Bind("MaCtdt,MaNhomHp,SoTcTuChon,SoTcBatBuoc")] XettotnghiepNhom model)
        {
            // Kiểm tra khóa
            if (maCtdt != model.MaCtdt || maNhomHp != model.MaNhomHp)
                return BadRequest("Dữ liệu không khớp.");

            if (!ModelState.IsValid)
            {
                ViewBag.MaCtdt = model.MaCtdt;
                ViewBag.MaNhomHp = model.MaNhomHp;
                ViewBag.TenNhomHp = (await _context.Nhomhps.FindAsync(model.MaNhomHp))?.TenNhomHp;
                return View(model);
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = await _context.XettotnghiepNhoms
                    .AnyAsync(x => x.MaCtdt == maCtdt && x.MaNhomHp == maNhomHp);
                if (!exists)
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("DanhSachMonTheoNhom", "Xettotnghiep", new { maCtdt = model.MaCtdt });
        }

        // GET: XettotnghiepNhom/Delete?maCtdt=CT02-63&maNhomHp=KTPM
        public async Task<IActionResult> Delete(string maCtdt, string maNhomHp)
        {
            if (string.IsNullOrEmpty(maCtdt) || string.IsNullOrEmpty(maNhomHp))
                return NotFound();

            var entity = await _context.XettotnghiepNhoms
                .Include(x => x.MaCtdtNavigation)
                .Include(x => x.MaNhomHpNavigation)
                .FirstOrDefaultAsync(x => x.MaCtdt == maCtdt && x.MaNhomHp == maNhomHp);

            if (entity == null)
                return NotFound();

            return View(entity);
        }

        // POST: XettotnghiepNhom/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string maCtdt, string maNhomHp)
        {
            var entity = await _context.XettotnghiepNhoms.FindAsync(maCtdt, maNhomHp);
            if (entity != null)
            {
                _context.XettotnghiepNhoms.Remove(entity);
                await _context.SaveChangesAsync();
            }
            // Quay về DanhSachMonTheoNhom với giữ lại maCtdt
            return RedirectToAction(
                actionName: "DanhSachMonTheoNhom",
                controllerName: "Xettotnghiep",
                routeValues: new { maCtdt = maCtdt }
            );
        }


        private bool XettotnghiepNhomExists(string id)
        {
            return _context.XettotnghiepNhoms.Any(e => e.MaCtdt == id);
        }
    }
}
