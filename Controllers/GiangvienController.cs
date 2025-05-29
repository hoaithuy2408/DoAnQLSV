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
    public class GiangvienController : Controller
    {
        private readonly HocbasvContext _context;

        public GiangvienController(HocbasvContext context)
        {
            _context = context;
        }

        // GET: Giangvien
        public async Task<IActionResult> Index(string searchTerm, string sortColumn, string sortOrder, int page = 1, int pageSize = 8)
        {
            var query = _context.Giangviens.Include(g => g.MaBmNavigation).AsQueryable();

            // Tìm kiếm theo tên giảng viên hoặc email
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(g => g.HoTen.Contains(searchTerm) || g.Email.Contains(searchTerm));
            }

            // Sắp xếp dữ liệu
            switch (sortColumn)
            {
                case "MaGv":
                    query = sortOrder == "asc" ? query.OrderBy(g => g.MaGv) : query.OrderByDescending(g => g.MaGv);
                    break;
                case "HoTen":
                    query = sortOrder == "asc" ? query.OrderBy(g => g.HoTen) : query.OrderByDescending(g => g.HoTen);
                    break;
                case "Email":
                    query = sortOrder == "asc" ? query.OrderBy(g => g.Email) : query.OrderByDescending(g => g.Email);
                    break;
                case "MaBm":
                    query = sortOrder == "asc" ? query.OrderBy(g => g.MaBm) : query.OrderByDescending(g => g.MaBm);
                    break;
                default:
                    query = query.OrderBy(g => g.MaGv); // Mặc định sắp xếp theo Mã giảng viên
                    break;
            }

            // Tổng số bản ghi
            int totalItems = await query.CountAsync();

            // Phân trang
            var giangviens = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Truyền dữ liệu vào ViewData
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;

            return View(giangviens);
        }

        // GET: Giangvien/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giangvien = await _context.Giangviens
                .Include(g => g.MaBmNavigation)
                .FirstOrDefaultAsync(m => m.MaGv == id);
            if (giangvien == null)
            {
                return NotFound();
            }

            return View(giangvien);
        }

        // GET: Giangvien/Create
        [Authorize(Roles = "Them")]
        public IActionResult Create()
        {
            ViewData["MaBm"] = new SelectList(_context.Bomons, "MaBm", "TenBm");
            return View();
        }

        // POST: Giangvien/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaGv,HoTen,GioiTinh,NgaySinh,QueQuan,Sdt,Email,TenDn,Password,MaBm")]
                                        Giangvien giangvien, IFormFile Anh)
        {
            // Kiểm tra xem Mã Giảng Viên đã tồn tại chưa
            bool isExist = await _context.Giangviens.AnyAsync(g => g.MaGv == giangvien.MaGv);
            if (isExist)
            {
                ModelState.AddModelError("MaGv", "Mã giảng viên đã tồn tại.");
            }

            // Kiểm tra trùng Email
            bool isEmailExist = await _context.Giangviens.AnyAsync(g => g.Email == giangvien.Email);
            if (isEmailExist)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
            }

            // Kiểm tra nếu không có ảnh được tải lên
            if (Anh == null || Anh.Length == 0)
            {
                giangvien.Anh = "/images/avt.jpg"; // Ảnh mặc định
            }
            else
            {
                // Xử lý lưu ảnh
                var fileName = Path.GetFileName(Anh.FileName); // Giữ nguyên tên file
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                // Nếu file chưa tồn tại, lưu nó
                if (!System.IO.File.Exists(uploadPath))
                {
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await Anh.CopyToAsync(stream);
                    }
                }

                giangvien.Anh = "/images/" + fileName; // Lưu đường dẫn vào DB
            }

            if (ModelState.IsValid)
            {
                giangvien.MaBmNavigation = null; // Tránh lỗi liên kết

                _context.Add(giangvien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaBm"] = new SelectList(_context.Bomons, "MaBm", "TenBm", giangvien.MaBm);
            return View(giangvien);
        }


        // GET: Giangvien/Edit/5
        [Authorize(Roles = "Sua")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giangvien = await _context.Giangviens.FindAsync(id);
            if (giangvien == null)
            {
                return NotFound();
            }
            ViewData["MaBm"] = new SelectList(_context.Bomons, "MaBm", "TenBm", giangvien.MaBm);
            return View(giangvien);
        }

        // POST: Giangvien/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaGv,HoTen,GioiTinh,NgaySinh,QueQuan,Sdt,Email,TenDn,Password,MaBm")]
                    Giangvien giangvien, IFormFile? Anh)
        {
            if (id != giangvien.MaGv)
            {
                return NotFound();
            }

            // Lấy thông tin giảng viên cũ từ database
            var existingGiangvien = await _context.Giangviens.FirstOrDefaultAsync(g => g.MaGv == id);
            if (existingGiangvien == null)
            {
                return NotFound();
            }

            // Xử lý dữ liệu rỗng
            giangvien.QueQuan = string.IsNullOrWhiteSpace(giangvien.QueQuan) ? null : giangvien.QueQuan;
            giangvien.Sdt = string.IsNullOrWhiteSpace(giangvien.Sdt) ? null : giangvien.Sdt;

            //  Xử lý ảnh (chỉ thay đổi nếu có ảnh mới)
            if (Anh != null && Anh.Length > 0)
            {
                var fileExtension = Path.GetExtension(Anh.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // Kiểm tra định dạng ảnh hợp lệ
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Anh", "Chỉ chấp nhận định dạng ảnh JPG, JPEG, PNG, GIF.");
                    ViewData["MaBm"] = new SelectList(_context.Bomons, "MaBm", "MaBm", giangvien.MaBm);
                    return View(giangvien);
                }

                var fileName = Path.GetFileName(Anh.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                // Xóa ảnh cũ nếu có và không phải ảnh mặc định
                if (!string.IsNullOrEmpty(existingGiangvien.Anh) && existingGiangvien.Anh != "/images/avt.jpg")
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingGiangvien.Anh.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Lưu ảnh mới
                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }

                existingGiangvien.Anh = "/images/" + fileName; // Cập nhật đường dẫn ảnh mới
            }
            else
            {
                // Nếu không có ảnh mới, giữ nguyên ảnh cũ
                existingGiangvien.Anh ??= "/images/avt.jpg";
            }

            // ✅ Chỉ cập nhật thuộc tính thay đổi, tránh ghi đè toàn bộ đối tượng
            existingGiangvien.HoTen = giangvien.HoTen;
            existingGiangvien.GioiTinh = giangvien.GioiTinh;
            existingGiangvien.NgaySinh = giangvien.NgaySinh;
            existingGiangvien.QueQuan = giangvien.QueQuan;
            existingGiangvien.Sdt = giangvien.Sdt;
            existingGiangvien.Email = giangvien.Email;
            existingGiangvien.TenDn = giangvien.TenDn;
            existingGiangvien.Password = giangvien.Password;
            existingGiangvien.MaBm = giangvien.MaBm;

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync(); // ✅ Chỉ lưu thay đổi thực tế
                    return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GiangvienExists(giangvien.MaGv))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["MaBm"] = new SelectList(_context.Bomons, "MaBm", "TenBm", giangvien.MaBm);
            return View(giangvien);
        }


        // GET: Giangvien/Delete/5
        [Authorize(Roles = "Xoa")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giangvien = await _context.Giangviens
                .Include(g => g.MaBmNavigation)
                .FirstOrDefaultAsync(m => m.MaGv == id);
            if (giangvien == null)
            {
                return NotFound();
            }

            return View(giangvien);
        }

        // POST: Giangvien/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var giangvien = await _context.Giangviens.FindAsync(id);
            if (giangvien != null)
            {
                _context.Giangviens.Remove(giangvien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GiangvienExists(string id)
        {
            return _context.Giangviens.Any(e => e.MaGv == id);
        }
    }
}
