using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QLSV.Helpers;
using QLSV.Models;
using QLSV.ViewModel;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QLSV.Services;
using QuestPDF.Fluent;
using HTMLQuestPDF.Extensions;

namespace QLSV.Areas.SV.Controllers
{
    [Area("SV")]
    public class TblSinhvienController : Controller
    {
        private readonly HocbasvContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailHelper _emailHelper;
        private readonly ILogger<TblSinhvienController> _logger;
        private readonly IViewRenderService _viewRenderService;

        public TblSinhvienController(HocbasvContext context, IConfiguration configuration, EmailHelper emailHelper, ILogger<TblSinhvienController> logger, IViewRenderService viewRenderService)
        {
            _context = context;
            _configuration = configuration;
            _emailHelper = emailHelper;
            _logger = logger;
            _viewRenderService = viewRenderService;

        }


        private async Task<bool> Validate(string secretKey, string recaptchaResponse)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("secret", secretKey),
            new KeyValuePair<string, string>("response", recaptchaResponse)
        });

                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                return jsonResponse.success == "true";
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginSV()
        {
            ViewBag.SiteKey = _configuration["ReCaptcha:SiteKey"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> LoginSV(string username, string password)
        {
            // Kiểm tra reCAPTCHA trước
            var recaptchaResponse = Request.Form["g-recaptcha-response"];
            var isValidCaptcha = await Validate(_configuration["ReCaptcha:SecretKey"], recaptchaResponse);

            if (!isValidCaptcha)
            {
                ModelState.AddModelError("", "Vui lòng xác thực reCAPTCHA");
                ViewBag.SiteKey = _configuration["ReCaptcha:SiteKey"];
                return View();
            }

            // Validate input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin"); 
                ViewBag.SiteKey = _configuration["ReCaptcha:SiteKey"];
                return View();
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(sv => sv.MaSv == username);
            if (sinhVien == null || sinhVien.Password != password)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                ViewBag.SiteKey = _configuration["ReCaptcha:SiteKey"];
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, sinhVien.MaSv),
        new Claim(ClaimTypes.Name, sinhVien.HoTen),
        new Claim(ClaimTypes.Email, sinhVien.Email ?? string.Empty),
        new Claim("AccountType", "SinhVien"),
        new Claim(ClaimTypes.Role, "SinhVien")

    };

            var identity = new ClaimsIdentity(claims, "SinhVienCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                "SinhVienCookie",
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                });

            return RedirectToAction("Index", "TblSinhvien");
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        public async Task<IActionResult> LogoutSV() 
        {
            await HttpContext.SignOutAsync("SinhVienCookie");
            return RedirectToAction("Index", "Home");
        }

        // GET: SV/TblSinhvien/ForgotPasswordSV
        public IActionResult ForgotPasswordSV()
        {
            ViewBag.SiteKey = _configuration["ReCaptcha:SiteKey"];
            return View();
        }

        // POST: SV/TblSinhvien/ForgotPasswordSV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordSV(string email)
        {
            // Lấy token reCAPTCHA từ form
            var recaptchaResponse = Request.Form["g-recaptcha-response"];
            var isCaptchaValid = await Validate(
                _configuration["ReCaptcha:SecretKey"],
                recaptchaResponse);

            if (!isCaptchaValid)
            {
                ModelState.AddModelError("", "Vui lòng xác thực reCAPTCHA.");
                return View();
            }

            var sinhVien = await _context.Sinhviens.FirstOrDefaultAsync(sv => sv.Email == email);
            if (sinhVien == null)
            {
                ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                return View();
            }

            // Generate reset token
            string resetToken = Guid.NewGuid().ToString();
            sinhVien.ResetToken = resetToken;
            sinhVien.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            _context.Update(sinhVien);
            await _context.SaveChangesAsync();

            // Generate reset link (lưu ý thêm area vào Url.Action)
            string resetLink = Url.Action("ResetPasswordSV", "TblSinhvien",
                new
                {
                    area = "SV",
                    token = resetToken
                },
                Request.Scheme);

            await _emailHelper.SendEmailAsync(email, "Đặt lại mật khẩu",
                $"Sinh viên nhấp vào link sau để đặt lại mật khẩu: <a href='{resetLink}'>{resetLink}</a>");

            TempData["SuccessMessage"] = "Một email đã được gửi đến bạn để tạo lại mật khẩu.";
            return RedirectToAction("LoginSV", "TblSinhvien", new { area = "SV" });
        }


        [HttpGet]
        public IActionResult ResetPasswordSV(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Truy cập không hợp lệ.");
            }

            var sinhvien = _context.Sinhviens.FirstOrDefault(sv => sv.ResetToken == token);
            if (sinhvien == null || sinhvien.ResetTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }

            ViewData["Token"] = token;
            ViewBag.SiteKey = _configuration["ReCaptcha:SiteKey"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordSV(string token, string password, string confirmPassword)
        {
            var recaptchaResponse = Request.Form["g-recaptcha-response"];
            if (!await Validate(_configuration["ReCaptcha:SecretKey"], recaptchaResponse))
            {
                ModelState.AddModelError("", "Vui lòng xác thực reCAPTCHA.");
                ViewData["Token"] = token;
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu và xác nhận không khớp.");
                ViewData["Token"] = token;
                return View();
            }

            
            var sinhvien = await _context.Sinhviens.FirstOrDefaultAsync(sv => sv.ResetToken == token && sv.ResetTokenExpiry > DateTime.UtcNow);
            if (sinhvien == null)
            {
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }

            sinhvien.Password = password;
            sinhvien.ResetToken = null;
            sinhvien.ResetTokenExpiry = null;

            _context.Sinhviens.Update(sinhvien);
            await _context.SaveChangesAsync();

            return RedirectToAction("LoginSV", "TblSinhvien");
        }

        

        // GET: Sinhvien
        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        public async Task<IActionResult> Index()
        {
            var hocbasvContext = _context.Sinhviens
                .Include(s => s.MaCtdtNavigation)
                .Include(s => s.MaKhoaNavigation)
                .Include(s => s.MaLopNavigation)
                .Include(s => s.MaNganhNavigation)
                .Include(s => s.MaNkNavigation);

            return View(await hocbasvContext.ToListAsync());
        }


        [HttpGet]
        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        public async Task<IActionResult> ThongTinSV()
        {
            var maSv = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maSv))
                return RedirectToAction("LoginSV", "Account");

            var sinhVien = await _context.Sinhviens
                .Include(s => s.MaKhoaNavigation)
                .Include(s => s.MaLopNavigation)
                .Include(s => s.MaNganhNavigation)
                .Include(s => s.MaNkNavigation)
                .Include(s => s.MaCtdtNavigation)
                .FirstOrDefaultAsync(s => s.MaSv == maSv);

            if (sinhVien == null)
                return NotFound();


            return View(sinhVien);
        }



        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        public IActionResult ChiTietChuongTrinhDaoTao(string? maNh, string? maHk)
        {
            var maSv = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maSv))
                return RedirectToAction("LoginSV", "TblSinhvien");

            var sinhvien = _context.Sinhviens
                .Include(s => s.MaCtdtNavigation)
                .Include(s => s.MaNkNavigation)
                .FirstOrDefault(s => s.MaSv == maSv);

            if (sinhvien == null)
                return NotFound("Không tìm thấy sinh viên");

            var dsCt = _context.CtCtdts
                .Include(c => c.MaMhNavigation)
                .Include(c => c.MaNhNavigation)
                .Include(c => c.MaHkNavigation)
                .Where(c => c.MaCtdt == sinhvien.MaCtdt)
                .ToList();

            // Lọc theo năm học và học kỳ nếu được truyền vào
            if (!string.IsNullOrEmpty(maNh))
                dsCt = dsCt.Where(c => c.MaNh == maNh).ToList();
            if (!string.IsNullOrEmpty(maHk))
                dsCt = dsCt.Where(c => c.MaHk == maHk).ToList();

            var ketQua = _context.Kqhts
                .Where(k => k.MaSv == maSv)
                .ToList();

            var viewModel = dsCt.Select(ct => new CTDTViewModel
            {
                MaMh = ct.MaMh,
                TenMh = ct.MaMhNavigation.TenMh,
                SoTc = ct.MaMhNavigation.SoTc,
                LoaiMh = ct.LoaiMh,
                MaNh = ct.MaNh,
                TenNh = ct.MaNhNavigation?.TenNh,
                MaHk = ct.MaHk,
                TenHk = ct.MaHkNavigation?.TenHk.ToString(),
                DiemTb = ketQua
                .Where(k => k.MaMh == ct.MaMh)
                .OrderByDescending(k => k.MaNh) 
                .ThenByDescending(k => k.MaHk)
                .Select(k => k.DiemTb)
                .FirstOrDefault()
                    }).ToList();


            // Gửi dữ liệu dropdown
            ViewBag.NamHocs = _context.Namhocs
                .OrderBy(nh => nh.MaNh)
                .Select(nh => new SelectListItem
                {
                    Value = nh.MaNh,
                    Text = nh.TenNh,
                    Selected = (nh.MaNh == maNh)
                }).ToList();

            ViewBag.HocKys = _context.Hockies
                .OrderBy(hk => hk.MaHk)
                .Select(hk => new SelectListItem
                {
                    Value = hk.MaHk,
                    Text = hk.MaHk,
                    Selected = (hk.MaHk == maHk)
                }).ToList();

            ViewBag.CurrentNamHoc = maNh;
            ViewBag.CurrentHocKy = maHk;

            return View("ChiTietCTDT_SV", viewModel);
        }


        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        public async Task<IActionResult> ChiTiet_KHHTSV(string? maNh, string? maHk, bool? dangKyMode)
        {
            // 0. Lấy mã sinh viên hiện tại
            var maSv = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maSv))
                return RedirectToAction("LoginSV", "Account");

            // 1. Lấy sinh viên và niên khóa
            var sinhvien = await _context.Sinhviens
                .Include(s => s.MaNkNavigation)
                .Include(s => s.MaCtdtNavigation)
                .FirstOrDefaultAsync(s => s.MaSv == maSv);
            if (sinhvien == null)
                return NotFound("Không tìm thấy sinh viên");

            // 2. Tách năm bắt đầu và kết thúc của niên khóa
            var nkParts = sinhvien.MaNkNavigation.TenNk.Split('-');
            int nkStart = int.Parse(nkParts[0]);
            int nkEnd = int.Parse(nkParts[1]);

            // 3. Nếu chưa truyền maNh/maHk, tìm kỳ đang mở
            if (string.IsNullOrEmpty(maNh) && string.IsNullOrEmpty(maHk))
            {
                var now = DateTime.Now;
                var openList = await _context.Thoigiandangkies
                    .Where(t => t.NgayBatDau <= now
                             && now <= t.NgayKetThuc
                             && t.ChoPhepDangKy)
                    .ToListAsync();

                if (openList.Any())
                {
                    var latest = openList.OrderByDescending(t => t.NgayBatDau).First();
                    maNh = latest.MaNh;
                    maHk = latest.MaHk;
                }
                else
                {
                    ViewBag.ErrorMessage = "Chưa có kỳ đăng ký; đang hiển thị tất cả môn học.";
                }
            }

            // 4. Khởi tạo dropdown năm học và học kỳ
            ViewBag.NamHocs = (await _context.Namhocs.ToListAsync())
                .Select(n => new SelectListItem
                {
                    Value = n.MaNh,
                    Text = n.TenNh,
                    Selected = (n.MaNh == maNh)
                })
                .ToList();

            ViewBag.HocKys = (await _context.Hockies
                .OrderBy(h => h.MaHk)
                .ToListAsync())
                .Select(h => new SelectListItem
                {
                    Value = h.MaHk,
                    Text = h.MaHk,
                    Selected = (h.MaHk == maHk)
                })
                .ToList();

            // 5. Lấy danh sách KHHT của sinh viên
            var khhts = await _context.Khhts
                .Where(k => k.MaCtdt == sinhvien.MaCtdt
                         && k.MaNk == sinhvien.MaNk)
                .ToListAsync();

            if (!khhts.Any())
                return NotFound("Không có kế hoạch học tập phù hợp");

            var khhtMap = khhts.ToDictionary(k => k.MaKhht, k => new { k.MaNh, k.MaHk });
            var khhtIds = khhts.Select(k => k.MaKhht).ToList();

            // 6. Tạo bản ghi CtKhhtSv nếu chưa có
            var allCt = await _context.CtKhhts
                .Where(ct => khhtIds.Contains(ct.MaKhht))
                .ToListAsync();

            foreach (var ct in allCt)
            {
                bool exists = await _context.CtKhhtSvs.AnyAsync(sv =>
                    sv.MaSv == maSv &&
                    sv.MaKhht == ct.MaKhht &&
                    sv.MaMh == ct.MaMh);

                if (!exists)
                {
                    var info = khhtMap[ct.MaKhht];
                    _context.CtKhhtSvs.Add(new CtKhhtSv
                    {
                        MaSv = maSv,
                        MaKhht = ct.MaKhht,
                        MaMh = ct.MaMh,
                        MaNh = info.MaNh,
                        MaHk = info.MaHk,
                        LoaiMh = ct.LoaiMh ?? false,
                        XacNhanDk = false
                    });
                }
            }
            await _context.SaveChangesAsync();

            // 7. Xây dựng query với 4 trường hợp lọc
            var query = _context.CtKhhtSvs
                .Include(ct => ct.MaMhNavigation)
                .Include(ct => ct.MaKhhtNavigation)
                .Where(ct => ct.MaSv == maSv && khhtIds.Contains(ct.MaKhht));

            bool hasMaNh = !string.IsNullOrEmpty(maNh);
            bool hasMaHk = !string.IsNullOrEmpty(maHk);

            if (hasMaNh && !hasMaHk)
            {
                // chỉ lọc theo năm học
                query = query.Where(ct => ct.MaNh == maNh);
            }
            else if (!hasMaNh && hasMaHk)
            {
                // chỉ lọc theo học kỳ
                query = query.Where(ct => ct.MaHk == maHk);
            }
            else if (hasMaNh && hasMaHk)
            {
                // lọc theo cả năm học và học kỳ
                query = query.Where(ct => ct.MaNh == maNh && ct.MaHk == maHk);

                var currentKhht = khhts.FirstOrDefault(k => k.MaNh == maNh && k.MaHk == maHk);
                if (currentKhht != null)
                {
                    ViewBag.CurrentMaKhht = currentKhht.MaKhht;
                }
                else
                {
                    ViewBag.CurrentMaKhht = null;
                    return View(new List<CtKhhtSv>());
                }
            }

            var result = await query
                .OrderBy(ct => ct.MaKhhtNavigation.MaNh)
                .ThenBy(ct => ct.MaKhhtNavigation.MaHk)
                .ThenBy(ct => ct.MaMh)
                .ToListAsync();

            // 8. Quyền đăng ký: chỉ nếu đang trong kỳ và cả maNh+maHk có
            bool duocDangKy = false;
            if (hasMaNh && hasMaHk)
            {
                var tgdk = await _context.Thoigiandangkies
                    .Include(t => t.MaNhNavigation)
                    .FirstOrDefaultAsync(t =>
                        t.MaNh == maNh && t.MaHk == maHk && t.ChoPhepDangKy);
                if (tgdk != null)
                {
                    int startYear = int.Parse(tgdk.MaNhNavigation.TenNh.Split('-')[0]);
                    duocDangKy = startYear >= nkStart && startYear < nkEnd;
                }
            }

            // 9. Đưa xuống View
            ViewBag.DangKyMode = dangKyMode ?? false;
            ViewBag.CurrentNamHoc = maNh;
            ViewBag.CurrentHocKy = maHk;
            ViewBag.DuocDangKy = duocDangKy;

            return View(result);
        }


        [HttpGet]
        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        public IActionResult GoiYKHHT(string maKhht)
        {
            // 1. Kiểm tra tham số & quyền
            if (string.IsNullOrEmpty(maKhht))
                return BadRequest("Thiếu MaKhht.");

            var maSv = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maSv))
                return Unauthorized();

            // 2. Lấy KHHT hiện tại
            var khht = _context.Khhts.SingleOrDefault(k => k.MaKhht == maKhht);
            if (khht == null)
                return NotFound("KHHT không tồn tại.");

            int maxTc = khht.SoTctong;
            int minTc = khht.SoTcbatBuoc;

            // 3. Tính tổng tín chỉ đã đăng ký trong kỳ này
            var registeredThis = _context.CtKhhtSvs
                .Where(ct => ct.MaSv == maSv && ct.MaKhht == maKhht && ct.XacNhanDk)
                .Select(ct => ct.MaMh)
                .ToList();

            var registeredCourses = _context.Monhocs
                .Where(m => registeredThis.Contains(m.MaMh))
                .ToList();

            int sumTc = registeredCourses.Sum(m => m.SoTc);

            int needTc = Math.Max(0, minTc - sumTc);

            // 4. Môn trong kỳ hiện tại chưa đăng ký
            var inThisSemester = _context.CtKhhts
                .Where(ct => ct.MaKhht == maKhht)
                .Select(ct => ct.MaMh)
                .ToList();

            var currentNotRegistered = _context.CtKhhts
            .Include(ct => ct.MaMhNavigation)
            .Where(ct => ct.MaKhht == maKhht && !registeredThis.Contains(ct.MaMh))
            .Select(ct => new CourseDto
            {
                  MaMh = ct.MaMh,
                  TenMh = ct.MaMhNavigation.TenMh,
                  SoTc = ct.MaMhNavigation.SoTc,
                  LoaiMh = ct.LoaiMh, 
                  Grade = null
            })
            .ToList();

            // 5. Xác định niên khóa trước
            var currNh = _context.Namhocs.Single(n => n.MaNh == khht.MaNh);
            var parts = currNh.TenNh.Split('-');
            int start = int.Parse(parts[0]), end = int.Parse(parts[1]);
            string prevTenNh = $"{start - 1}-{end - 1}";
            var prevNh = _context.Namhocs.SingleOrDefault(n => n.TenNh == prevTenNh);

            // 6. Môn trượt cùng kỳ trước (đã có đầy đủ điểm QT, GK, CK và TB < 5)
            List<CourseDto> prevFailed = new();
            if (prevNh != null)
            {
                prevFailed = (from k in _context.Kqhts
                    join ct in _context.CtCtdts on k.MaMh equals ct.MaMh
                    where k.MaSv == maSv
                    && k.MaNh == prevNh.MaNh
                    && k.MaHk == khht.MaHk
                    && ct.MaCtdt == khht.MaCtdt
                    && k.DiemTb < 5
                    && k.TyLeQt.HasValue
                    && k.TyLeGk.HasValue
                    && k.TyLeCk.HasValue
                    select new CourseDto
                     {
                          MaMh = k.MaMh,
                          TenMh = k.MaMhNavigation.TenMh,
                          SoTc = k.MaMhNavigation.SoTc,
                          LoaiMh = ct.LoaiMh,
                          Grade = (double?)k.DiemTb
                     })
                     .ToList();
            }

            // 7. Môn cùng kỳ năm trước chưa đăng ký
            List<CourseDto> prevYearNotRegistered = new();
            if (prevNh != null)
            {
                // Lưu ý: Giữ nguyên MaNk hiện tại, chỉ khác MaNh 
                var prevKhht = _context.Khhts
                    .FirstOrDefault(k => k.MaNk == khht.MaNk && k.MaNh == prevNh.MaNh && k.MaHk == khht.MaHk);

                if (prevKhht != null)
                {
                    var regPrevYear = _context.CtKhhtSvs
                        .Where(ct => ct.MaSv == maSv && ct.MaKhht == prevKhht.MaKhht && ct.XacNhanDk)
                        .Select(ct => ct.MaMh)
                        .ToList();

                    prevYearNotRegistered = _context.CtKhhts
                        .Include(ct => ct.MaMhNavigation)
                        .Where(ct => ct.MaKhht == prevKhht.MaKhht && !regPrevYear.Contains(ct.MaMh))
                        .Select(ct => new CourseDto
                        {
                            MaMh = ct.MaMh,
                            TenMh = ct.MaMhNavigation.TenMh,
                            SoTc = ct.MaMhNavigation.SoTc,
                            LoaiMh = ct.LoaiMh, 
                            Grade = null
                        })
                        .ToList();
                }
            }

            // 8. Chuẩn bị ViewModel
            var vm = new GoiYKHHT
            {
                MaKhht = maKhht,
                CurrentNotRegistered = currentNotRegistered,
                PrevFailed = prevFailed,
                PrevYearNotRegistered = prevYearNotRegistered
            };
            return View("GoiYKHHT", vm);
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        public IActionResult DangKyMonHoc([FromBody] DangKyMonHocRequest data)
        {
            var maSv = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maSv))
                return Json(new { success = false, message = "Thiếu mã sinh viên." });

            var allRegs = _context.CtKhhtSvs
                .Where(ct => ct.MaSv == maSv)
                .ToList();

            var warnings = new List<string>();
            var now = DateTime.Now;

            foreach (var course in data.Courses)
            {
                var parts = course.Key.Split('_');
                if (parts.Length != 2) continue;
                var maKhht = parts[0];
                var maMh = parts[1];

                var reg = allRegs.FirstOrDefault(r => r.MaKhht == maKhht && r.MaMh == maMh);
                if (reg == null) continue;

                bool oldSelected = reg.XacNhanDk;
                bool newSelected = course.Selected;

                // Lấy thông tin kỳ đăng ký (dựa trên năm + học kỳ của course)
                var period = _context.Thoigiandangkies
                    .SingleOrDefault(t => t.MaNh == course.MaNh && t.MaHk == course.MaHk);

                string? reason = null;

                // —— Kiểm tra ĐĂNG KÝ mới ——
                if (!oldSelected && newSelected)
                {
                    if (period == null)
                        reason = "chưa cấu hình thời gian đăng ký";
                    else if (!period.ChoPhepDangKy)
                        reason = "kỳ đăng ký đã đóng";
                    else if (now < period.NgayBatDau)
                        reason = $"chưa đến ngày bắt đầu ({period.NgayBatDau:dd/MM/yyyy})";
                    else if (now > period.NgayKetThuc)
                        reason = $"đã hết hạn ({period.NgayKetThuc:dd/MM/yyyy})";

                    if (reason != null)
                    {
                        warnings.Add($"{maMh}: {reason}");
                        continue; // không thực hiện đăng ký
                    }

                    // Thỏa điều kiện, đăng ký
                    reg.XacNhanDk = true;
                    reg.MaNh = course.MaNh;
                    reg.MaHk = course.MaHk;
                }
                // —— Kiểm tra HỦY đăng ký ——
                else if (oldSelected && !newSelected)
                {
                    if (period == null)
                        reason = "chưa cấu hình thời gian đăng ký";
                    else if (!period.ChoPhepDangKy)
                        reason = "kỳ đăng ký đã đóng";
                    else if (now < period.NgayBatDau)
                        reason = $"chưa đến ngày bắt đầu ({period.NgayBatDau:dd/MM/yyyy})";
                    else if (now > period.NgayKetThuc)
                        reason = $"đã hết hạn ({period.NgayKetThuc:dd/MM/yyyy})";

                    if (reason != null)
                    {
                        warnings.Add($"{maMh}: không thể hủy đăng ký – {reason}");
                        continue; // không thực hiện hủy
                    }

                    // Xóa kết quả học tập nếu đã nhập
                    var existingResults = _context.Kqhts
                        .Where(k => k.MaSv == maSv
                                 && k.MaMh == maMh
                                 && k.MaNh == reg.MaNh
                                 && k.MaHk == reg.MaHk)
                        .ToList();
                    if (existingResults.Any())
                        _context.Kqhts.RemoveRange(existingResults);

                    // Thỏa điều kiện, hủy đăng ký
                    reg.XacNhanDk = false;
                }
                // Nếu không thay đổi trạng thái (giữ nguyên oldSelected == newSelected) thì bỏ qua
            }

            _context.SaveChanges();

            if (warnings.Any())
            {
                return Json(new
                {
                    success = true,
                    message = "Cập nhật thành công, nhưng có một số môn không thực hiện được:",
                    warnings
                });
            }
            else
            {
                return Json(new { success = true, message = "Cập nhật đăng ký môn học thành công!" });
            }
        }

        //lấy kết quả học tập
        private async Task<List<KetQuaHocTapViewModel>> GetKetQuaHocTapSinhVienAsync(string maSv)
        {
            if (string.IsNullOrEmpty(maSv))
                return new List<KetQuaHocTapViewModel>();

            var sample = await _context.Kqhts
                .Where(k => k.MaSv == maSv)
                .Select(k => new { k.TyLeQt, k.TyLeGk, k.TyLeCk })
                .FirstOrDefaultAsync();

            double tyLeQt = sample?.TyLeQt ?? 0;
            double tyLeGk = sample?.TyLeGk ?? 0;
            double tyLeCk = sample?.TyLeCk ?? 0;

            var query = from ct in _context.CtKhhtSvs
                            .Include(ct => ct.MaMhNavigation)
                            .Include(ct => ct.MaNhNavigation)
                            .Include(ct => ct.MaHkNavigation)
                        where ct.MaSv == maSv && ct.XacNhanDk == true
                        join kq in _context.Kqhts.Where(k => k.MaSv == maSv)
                            on new { ct.MaSv, ct.MaMh, ct.MaNh, ct.MaHk }
                            equals new { kq.MaSv, kq.MaMh, kq.MaNh, kq.MaHk }
                            into gj
                        from kq in gj.DefaultIfEmpty()
                        orderby ct.MaNh, ct.MaHk, ct.MaMh
                        select new KetQuaHocTapViewModel
                        {
                            MaMh = ct.MaMh,
                            TenMh = ct.MaMhNavigation.TenMh,
                            MaNh = ct.MaNh,
                            TenNh = ct.MaNhNavigation.TenNh,
                            MaHk = ct.MaHk,
                            HocKy = ct.MaHkNavigation.TenHk,
                            SoTc = ct.MaMhNavigation.SoTc,
                            TyLeQt = tyLeQt,
                            TyLeGk = tyLeGk,
                            TyLeCk = tyLeCk,
                            DiemQt = kq != null ? kq.DiemQt : (double?)null,
                            DiemGk = kq != null ? kq.DiemGk : (double?)null,
                            DiemCk = kq != null ? kq.DiemCk : (double?)null,
                        };

            return await query.ToListAsync();
        }

        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        [HttpGet]
        public async Task<IActionResult> KetQuaHocTapSV()
        {
            var maSv = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maSv))
                return Unauthorized();

            var vmList = await GetKetQuaHocTapSinhVienAsync(maSv);

            return View("KetQuaHocTapSV", vmList); // hoặc PartialView nếu là trang con
        }

        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        [HttpGet]
        public async Task<IActionResult> XuatKetQuaHocTapSVPdf()
        {
            // 1. Lấy mã sinh viên từ claim
            var maSv = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(maSv))
                return Unauthorized();

            // 2. Lấy dữ liệu kết quả học tập của SV
            var vmList = await GetKetQuaHocTapSinhVienAsync(maSv);

            // 3. Đặt flag để ẩn nút và styling PDF nếu cần
            ViewData["IsExportPdf"] = true;
            string html = await _viewRenderService.RenderViewAsync("KetQuaHocTapSV", vmList,false);

            // 4. Tạo file PDF từ HTML
            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20, Unit.Millimetre);
                    page.DefaultTextStyle(t => t.FontSize(11));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().Text("BỘ GIÁO DỤC VÀ ĐÀO TẠO").FontSize(12);
                            col.Item().AlignCenter().Text("TRƯỜNG ĐẠI HỌC NHA TRANG").Bold().FontSize(12);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold().FontSize(12);
                            col.Item().AlignCenter().Text("Độc lập - Tự do - Hạnh phúc").Italic().FontSize(11);
                        });
                    });

                    // Nội dung HTML
                    page.Content().PaddingVertical(5).Column(col =>
                    {
                        col.Item().AlignCenter().Text("Bảng ghi điểm học phần").Bold().FontSize(13).Underline();
                        col.Item().PaddingTop(10);

                        col.Item().HTML(h =>
                        {
                            h.SetHtml(html);
                            h.SetTextStyleForHtmlElement("th", TextStyle.Default.FontSize(11).Bold());
                            h.SetTextStyleForHtmlElement("td", TextStyle.Default.FontSize(11));
                            h.SetTextStyleForHtmlElement("h5", TextStyle.Default.FontSize(12).Bold());
                        });

                        col.Item().Height(20);
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();

            // 5. Trả file
            return File(pdfBytes, "application/pdf", $"KetQuaHocTap_{maSv}.pdf");
        }



        [Authorize(AuthenticationSchemes = "SinhVienCookie")]
        [HttpGet]
        public async Task<IActionResult> XetTotNghiepSV()
        {
            var maSv = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maSv))
                return Unauthorized();

            var sv = await _context.Sinhviens
                .Where(s => s.MaSv == maSv)
                .Select(s => new { s.MaSv, s.HoTen, s.MaCtdt })
                .FirstOrDefaultAsync();

            if (sv == null)
                return NotFound("Không tìm thấy sinh viên");

            string maCtDt = sv.MaCtdt;

            var tc = await _context.Xettotnghieps.FirstOrDefaultAsync(x => x.MaCtdt == maCtDt);
            if (tc == null)
                return NotFound("Chưa cấu hình tiêu chí tốt nghiệp");

            const double NGUONG_DAT = 5.0;

            var nhomRequirements = await _context.XettotnghiepNhoms
               .Where(x => x.MaCtdt == maCtDt)
               .Select(x => new { x.MaNhomHp, x.SoTcBatBuoc, x.SoTcTuChon })
               .ToListAsync();

            var ketQuaHocTap = await (
                from kq in _context.Kqhts.Where(kq => kq.MaSv == maSv && kq.DiemTb.HasValue)
                join reg in _context.CtKhhtSvs.Where(r => r.MaSv == maSv && r.XacNhanDk)
                    on new { kq.MaSv, kq.MaMh } equals new { reg.MaSv, reg.MaMh }
                join ct in _context.CtCtdts.Where(ct => ct.MaCtdt == maCtDt)
                    on kq.MaMh equals ct.MaMh
                join mh in _context.Monhocs on kq.MaMh equals mh.MaMh
                select new
                {
                    mh.MaMh,
                    mh.TenMh,
                    mh.SoTc,
                    ct.MaNhomHp,
                    LoaiMh = ct.LoaiMh,
                    DiemTb = kq.DiemTb.Value,
                    Dat = kq.DiemTb.Value >= NGUONG_DAT
                }).ToListAsync();

            int soTCTichLuy = ketQuaHocTap.Where(x => x.Dat).Sum(x => x.SoTc);
            int soTCBatBuocDat = ketQuaHocTap.Where(x => x.LoaiMh && x.Dat).Sum(x => x.SoTc);
            int soTCTuChonDat = ketQuaHocTap.Where(x => !x.LoaiMh && x.Dat).Sum(x => x.SoTc);
            int soMonChuaDat = ketQuaHocTap.Count(x => !x.Dat);
            double tongTrongSo = ketQuaHocTap.Where(x => x.Dat).Sum(x => x.DiemTb * x.SoTc);
            double gpaTichLuy = soTCTichLuy > 0 ? tongTrongSo / soTCTichLuy : 0.0;

            bool datDieuKienTc = soTCTichLuy >= tc.SoTcTong;
            bool datDieuKienTcBatBuoc = soTCBatBuocDat >= tc.SoTcBatBuoc;
            bool datDieuKienTcTuChon = soTCTuChonDat >= tc.SoTcTuChon;
            bool datDieuKienGpa = gpaTichLuy >= (double)tc.GpatoiThieu;
            bool datMon = ketQuaHocTap.Where(x => x.LoaiMh).All(x => x.Dat);

            var datTheoNhom = new Dictionary<string, bool>();
            foreach (var req in nhomRequirements)
            {
                var nhom = ketQuaHocTap.Where(x => x.MaNhomHp == req.MaNhomHp);

                bool tatCaBatBuocDat = nhom
                    .Where(x => x.LoaiMh)
                    .All(x => x.Dat);

                int tuChonDat = nhom
                    .Where(x => !x.LoaiMh && x.Dat)
                    .Sum(x => x.SoTc);

                bool datTuChon = tuChonDat >= req.SoTcTuChon;

                datTheoNhom[req.MaNhomHp] = tatCaBatBuocDat && datTuChon;
            }

            var allCourses = await _context.CtCtdts
                .Where(ct => ct.MaCtdt == maCtDt)
                .Include(ct => ct.MaMhNavigation)
                .Select(ct => new {
                    ct.MaMh,
                    ct.MaNhomHp,
                    TenMh = ct.MaMhNavigation.TenMh,
                    SoTc = ct.MaMhNavigation.SoTc,
                    LoaiMh = ct.LoaiMh ? "Bắt buộc" : "Tự chọn"
                }).ToListAsync();

            var registeredMa = ketQuaHocTap.Select(x => x.MaMh).ToHashSet();

            var notRegistered = allCourses
                .Where(c => !registeredMa.Contains(c.MaMh))
                .Select(c => new MonHocChuaHoanThanhViewModel
                {
                    MaNhomHp = c.MaNhomHp,
                    MaMh = c.MaMh,
                    TenMh = c.TenMh,
                    SoTc = c.SoTc,
                    LoaiMh = c.LoaiMh,
                    TrangThai = "Chưa đăng ký"
                });

            var failed = ketQuaHocTap
                .Where(x => !x.Dat && (x.LoaiMh || !datTheoNhom[x.MaNhomHp]))
                .Select(x => new MonHocChuaHoanThanhViewModel
                {
                    MaNhomHp = x.MaNhomHp,
                    MaMh = x.MaMh,
                    TenMh = x.TenMh,
                    SoTc = x.SoTc,
                    LoaiMh = x.LoaiMh ? "Bắt buộc" : "Tự chọn",
                    TrangThai = $"Chưa đạt (Điểm: {x.DiemTb:0.00})"
                });

            var vm = new XetTotNghiepViewModel
            {
                MaSv = sv.MaSv,
                HoTen = sv.HoTen,
                MaCTDT = maCtDt,
                SoTCYeuCau = tc.SoTcTong,
                SoTC_BatBuoc = tc.SoTcBatBuoc,
                SoTC_TuChon = tc.SoTcTuChon,
                GpaYeuCau = (double)tc.GpatoiThieu,
                SoTCDaTichLuy = soTCTichLuy,
                SoTCBatBuocDat = soTCBatBuocDat,
                SoTCTuChonDat = soTCTuChonDat,
                GpaTichLuy = Math.Round(gpaTichLuy, 2),
                SoMonChuaDat = soMonChuaDat,
                DatTCTong = datDieuKienTc,
                DatTCBatBuoc = datDieuKienTcBatBuoc,
                DatTCTuChon = datDieuKienTcTuChon,
                DatGPA = datDieuKienGpa,
                DatMon = datMon,
                DatTheoNhom = datTheoNhom,
                DatTotNghiep = datDieuKienTc && datDieuKienTcBatBuoc && datDieuKienTcTuChon && datDieuKienGpa && datMon && datTheoNhom.Values.All(v => v),
                MonChuaHoanThanh = notRegistered.Concat(failed).ToList()
            };
            var allNhom = await _context.Nhomhps.ToDictionaryAsync(n => n.MaNhomHp, n => n.TenNhomHp);

            vm.NhomHocPhan = nhomRequirements.Select(req => {
                var monTrongNhom = vm.MonChuaHoanThanh.Where(m => m.MaNhomHp == req.MaNhomHp).ToList();
                return new NhomHocPhanViewModel
                {
                    MaNhomHp = req.MaNhomHp,
                    TenNhomHp = allNhom.GetValueOrDefault(req.MaNhomHp, req.MaNhomHp),
                    SoTcBatBuocCan = req.SoTcBatBuoc,
                    SoTcTuChonCan = req.SoTcTuChon,
                    SoTcBatBuocDat = ketQuaHocTap.Where(x => x.MaNhomHp == req.MaNhomHp && x.LoaiMh && x.Dat).Sum(x => x.SoTc),
                    SoTcTuChonDat = ketQuaHocTap.Where(x => x.MaNhomHp == req.MaNhomHp && !x.LoaiMh && x.Dat).Sum(x => x.SoTc),
                    Dat = datTheoNhom[req.MaNhomHp],
                    MonHoc = monTrongNhom
                };
            }).ToList();

            return View(vm);
        }

        private bool SinhvienExists(string id)
        {
            return _context.Sinhviens.Any(e => e.MaSv == id);
        }

    }
}
