using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSV.Models;
using PagedList;


using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QLSV.ViewModel;

namespace QLSV.Controllers
{
    public class ChiTietCtdtController : Controller
    {
        private readonly HocbasvContext _context;
        private readonly ILogger<ChiTietCtdtController> _logger;

        public ChiTietCtdtController(HocbasvContext context, ILogger<ChiTietCtdtController> logger)
    {
        _context = context;
        _logger = logger;
    }

        [HttpGet]
        public async Task<IActionResult> BulkCreate(string maCtdt)
        {
            try
            {
                // Lấy thông tin CTDT (không cần check null vì đã validate)
                var ctdt = await _context.Ctdts
                    .AsNoTracking()
                    .FirstAsync(c => c.MaCtdt == maCtdt); // Sử dụng FirstAsync thay vì FirstOrDefaultAsync

                // Lấy dữ liệu học kì cho dropdown
                var hockys = await _context.Hockies
                    .Select(h => new SelectListItem(h.MaHk, h.MaHk))
                    .ToListAsync();

                // Lấy danh sách Năm học cho dropdown
                var namhocs = await _context.Namhocs
                    .OrderByDescending(n => n.MaNh)
                    .Select(n => new SelectListItem(n.TenNh, n.MaNh))
                    .ToListAsync();

                // Lấy danh sách NhomHP cho dropdown
                var nhomHps = await _context.Nhomhps
                    .Select(n => new SelectListItem(n.TenNhomHp, n.MaNhomHp))
                    .ToListAsync();


                // Lấy danh sách môn học
                var allMonHoc = await _context.Monhocs.ToListAsync();
                var existingMappings = await _context.CtCtdts
                    .Where(ct => ct.MaCtdt == maCtdt)
                    .ToListAsync();

                // Tạo view model
                var model = new BulkCreateViewModel
                {
                    MaCtdt = maCtdt,
                    LoaiMhList = new List<SelectListItem>
            {
                new SelectListItem("Bắt buộc", "true"),
                new SelectListItem("Tự chọn", "false")
            },
                    HkList = hockys,
                    NhList = namhocs,
                    NhomHpList = nhomHps,
                    MonHocSelections = allMonHoc.Select(mh => new MonHocSelection
                    {
                        MaMh = mh.MaMh,
                        TenMh = mh.TenMh,
                        IsSelected = existingMappings.Any(ct => ct.MaMh == mh.MaMh),
                        LoaiMh = existingMappings.FirstOrDefault(ct => ct.MaMh == mh.MaMh)?.LoaiMh,
                        MaHk = existingMappings.FirstOrDefault(ct => ct.MaMh == mh.MaMh)?.MaHk,
                        MaNh = existingMappings.FirstOrDefault(ct => ct.MaMh == mh.MaMh)?.MaNh,
                        MaNhomHp = existingMappings.FirstOrDefault(ct => ct.MaMh == mh.MaMh)?.MaNhomHp 

                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Lỗi khi tải trang BulkCreate");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi khởi tạo form";
                return RedirectToAction("Details", "Ctdt", new { id = maCtdt });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(BulkCreateViewModel model)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _logger.LogInformation($"Bắt đầu BulkCreate cho CTDT {model.MaCtdt}");

                    var selectedSubjects = model.MonHocSelections.Where(m => m.IsSelected).ToList();

                    foreach (var subject in selectedSubjects)
                    {
                        var existing = await _context.CtCtdts
                            .FirstOrDefaultAsync(ct => ct.MaCtdt == model.MaCtdt && ct.MaMh == subject.MaMh);

                        if (existing != null)
                        {
                            existing.LoaiMh = subject.LoaiMh ?? true;
                            existing.MaHk = subject.MaHk ?? "HK1";
                            existing.MaNh = subject.MaNh ?? DateTime.Now.Year.ToString();
                            existing.MaNhomHp = subject.MaNhomHp ?? existing.MaNhomHp;
                            _logger.LogInformation($"Cập nhật môn {subject.MaMh}");
                        }
                        else
                        {
                            var newItem = new CtCtdt
                            {
                                MaCtdt = model.MaCtdt,
                                MaMh = subject.MaMh,
                                LoaiMh = subject.LoaiMh ?? true,
                                MaHk = subject.MaHk ?? "HK1",
                                MaNh = subject.MaNh ?? DateTime.Now.Year.ToString(),
                                MaNhomHp = subject.MaNhomHp ?? "DEFAULT_NHOM"  

                            };
                            await _context.CtCtdts.AddAsync(newItem);
                            _logger.LogInformation($"Thêm mới môn {subject.MaMh}");
                        }
                    }

                    // Xóa các môn không được chọn
                    var toRemove = await _context.CtCtdts
                        .Where(ct => ct.MaCtdt == model.MaCtdt &&
                                    !selectedSubjects.Select(m => m.MaMh).Contains(ct.MaMh))
                        .ToListAsync();

                    if (toRemove.Any())
                    {
                        _context.CtCtdts.RemoveRange(toRemove);
                        _logger.LogInformation($"Đã xóa {toRemove.Count} môn học");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    _logger.LogInformation("BulkCreate thành công");

                    return Json(new
                    {
                        success = true,
                        redirectUrl = Url.Action("Details", "Ctdt", new
                        {
                            id = model.MaCtdt,
                            activeTab = "chitiet-content"
                        })
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Lỗi khi thực hiện BulkCreate");
                    return Json(new
                    {
                        success = false,
                        error = "Đã xảy ra lỗi khi xử lý: " + ex.Message
                    });
                }
            }
        }
        // GET: ChiTietCtdt
        [HttpGet]
        public async Task<IActionResult> LoadChiTiet(string maCtdt, int? page)
        {
            if (string.IsNullOrEmpty(maCtdt))
            {
                return BadRequest("Mã CTĐT không hợp lệ");
            }

            int pageSize = 10; // Số lượng mục trên mỗi trang
            int pageNumber = page ?? 1;

            ViewBag.MaCtdt = maCtdt;

            var chiTietList = await _context.CtCtdts
            .Where(ct => ct.MaCtdt == maCtdt)
            .Include(ct => ct.MaMhNavigation) 
            .Include(ct => ct.MaNhNavigation) 
            .Include(ct => ct.MaHkNavigation)
            .Include(ct => ct.MaNhomHpNavigation)

            .OrderBy(ct => ct.MaMh)
            .ToListAsync();

            var pagedList = chiTietList.ToPagedList(pageNumber, pageSize); // Áp dụng phân trang
            return PartialView("_ChiTietCTDT", pagedList); // <-- dùng pagedList
        }


        public async Task<IActionResult> Index(string searchTerm, string sortColumn, string sortOrder, int page = 1, int pageSize = 8)
        {
            var query = _context.CtCtdts
                .Include(c => c.MaCtdtNavigation)
                .Include(c => c.MaHkNavigation)
                .Include(c => c.MaMhNavigation)
                .Include(c => c.MaNhNavigation)
                .AsQueryable();

            // Tìm kiếm theo mã CTĐT, mã học kỳ, mã môn học hoặc mã nhóm học
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    c.MaCtdtNavigation.MaCtdt.Contains(searchTerm) ||
                    c.MaHkNavigation.MaHk.Contains(searchTerm) ||
                    c.MaMhNavigation.MaMh.Contains(searchTerm) ||
                    c.MaNhNavigation.MaNh.Contains(searchTerm));
            }

            // Sắp xếp dữ liệu
            switch (sortColumn)
            {
                case "MaCtdt":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.MaCtdtNavigation.MaCtdt) : query.OrderByDescending(c => c.MaCtdtNavigation.MaCtdt);
                    break;
                case "MaHk":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.MaHkNavigation.MaHk) : query.OrderByDescending(c => c.MaHkNavigation.MaHk);
                    break;
                case "MaMh":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.MaMhNavigation.MaMh) : query.OrderByDescending(c => c.MaMhNavigation.MaMh);
                    break;
                case "MaNh":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.MaNhNavigation.MaNh) : query.OrderByDescending(c => c.MaNhNavigation.MaNh);
                    break;
                default:
                    query = query.OrderBy(c => c.MaCtdtNavigation.MaCtdt); // Mặc định sắp xếp theo Mã CTĐT
                    break;
            }

            // Tổng số bản ghi
            int totalItems = await query.CountAsync();

            // Phân trang
            var ctCtdts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Truyền dữ liệu vào ViewData
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;

            return View(ctCtdts);
        }


        // GET: ChiTietCtdt/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ctCtdt = await _context.CtCtdts
                .Include(c => c.MaCtdtNavigation)
                .Include(c => c.MaHkNavigation)
                .Include(c => c.MaMhNavigation)
                .Include(c => c.MaNhNavigation)
                .FirstOrDefaultAsync(m => m.MaCtdt == id);
            if (ctCtdt == null)
            {
                return NotFound();
            }

            return View(ctCtdt);
        }

        // GET: ChiTietCtdt/Create
        public IActionResult Create()
        {
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "MaCtdt");
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk");
            ViewData["MaMh"] = new SelectList(_context.Monhocs, "MaMh", "MaMh");
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "MaNh");
            return View();
        }

        // POST: ChiTietCtdt/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCtdt,MaNh,MaHk,MaMh,LoaiMh")] CtCtdt ctCtdt)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ctCtdt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "MaCtdt", ctCtdt.MaCtdt);
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "MaHk", ctCtdt.MaHk);
            ViewData["MaMh"] = new SelectList(_context.Monhocs, "MaMh", "MaMh", ctCtdt.MaMh);
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "MaNh", ctCtdt.MaNh);
            return View(ctCtdt);
        }

        // GET: ChiTietCtdt/Edit/5
        public async Task<IActionResult> Edit(string maCtdt, string maMh)
        {
            if (maCtdt == null || maMh == null)
            {
                return NotFound();
            }

            var ctCtdt = await _context.CtCtdts
                .FirstOrDefaultAsync(m => m.MaCtdt == maCtdt && m.MaMh == maMh);

            if (ctCtdt == null)
            {
                return NotFound();
            }
            var monHoc = _context.Monhocs.FirstOrDefault(m => m.MaMh == ctCtdt.MaMh);
            ViewBag.TenMh = monHoc?.TenMh ?? "Không xác định";

            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "TenCtdt", ctCtdt.MaCtdt);
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "TenHk", ctCtdt.MaHk);
            ViewData["MaMh"] = new SelectList(_context.Monhocs, "MaMh", "TenMh", ctCtdt.MaMh);
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh", ctCtdt.MaNh);

            return View(ctCtdt);
        }


        // POST: ChiTietCtdt/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string maCtdt, string maMh, [Bind("MaCtdt,MaNh,MaHk,MaMh,LoaiMh")] CtCtdt ctCtdt)
        {
            if (maCtdt != ctCtdt.MaCtdt || maMh != ctCtdt.MaMh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Attach(ctCtdt);
                    _context.Entry(ctCtdt).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!CtCtdtExists(ctCtdt.MaCtdt, ctCtdt.MaMh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Ctdt", new { id = maCtdt, tab = "chitiet-content" });
            }
            var monHoc = _context.Monhocs.FirstOrDefault(m => m.MaMh == ctCtdt.MaMh);
            ViewBag.TenMh = monHoc?.TenMh ?? "Không xác định";
            Console.WriteLine($"LoaiMh nhận được: {ctCtdt.LoaiMh}");


            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "TenCtdt", ctCtdt.MaCtdt);
            ViewData["MaHk"] = new SelectList(_context.Hockies, "MaHk", "TenHk", ctCtdt.MaHk);
            ViewData["MaMh"] = new SelectList(_context.Monhocs, "MaMh", "TenMh", ctCtdt.MaMh);
            ViewData["MaNh"] = new SelectList(_context.Namhocs, "MaNh", "TenNh", ctCtdt.MaNh);

            return RedirectToAction("Details", "Ctdt", new { id = maCtdt });
        }

        // GET: ChiTietCtdt/Delete/5
        public async Task<IActionResult> Delete(string maCtdt, string maMh)
        {
            if (maCtdt == null || maMh == null)
            {
                return NotFound();
            }

            var ctCtdt = await _context.CtCtdts
                .Include(c => c.MaCtdtNavigation)
                .Include(c => c.MaHkNavigation)
                .Include(c => c.MaMhNavigation)
                .Include(c => c.MaNhNavigation)
                .FirstOrDefaultAsync(m => m.MaCtdt == maCtdt && m.MaMh == maMh);

            if (ctCtdt == null)
            {
                return NotFound();
            }

            return View(ctCtdt);
        }


        // POST: ChiTietCtdt/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string maCtdt, string maMh)
        {
            var ctCtdt = await _context.CtCtdts.FindAsync(maCtdt, maMh);
            if (ctCtdt != null)
            {
                _context.CtCtdts.Remove(ctCtdt);
                await _context.SaveChangesAsync();
            }

            // Chuyển hướng về trang gốc mà không làm mất tab đang mở
            return RedirectToAction("Details", "Ctdt", new { id = maCtdt });
        }


        private bool CtCtdtExists(string maCtdt, string maMh)
        {
            return _context.CtCtdts.Any(e => e.MaCtdt == maCtdt && e.MaMh == maMh);
        }

    }
}
