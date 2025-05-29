using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QLSV.Models;
using QLSV.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace QLSV.Controllers
{
    public class AdminController : Controller
    {
        private readonly HocbasvContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailHelper _emailHelper;

        public AdminController(HocbasvContext context, IConfiguration configuration, EmailHelper emailHelper)
        {
            _context = context;
            _configuration = configuration;
            _emailHelper = emailHelper;
        }

        // Xác thực capcha
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
                return jsonResponse.success == "true"; // Kiểm tra xem reCAPTCHA có hợp lệ không
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Login()
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.SiteKey = siteKey;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];

            // Tìm giảng viên theo mã GV (username)
            var giangVien = await _context.Giangviens
                .Include(gv => gv.QuyenGVs)
                .ThenInclude(qg => qg.Quyen)
                .FirstOrDefaultAsync(gv => gv.MaGv == username);

            if (giangVien == null || giangVien.Password != password)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                ViewBag.SiteKey = siteKey;
                return View();
            }

            // Kiểm tra reCAPTCHA
            var recaptchaSecretKey = _configuration["ReCaptcha:SecretKey"];
            var recaptchaResponseVal = Request.Form["g-recaptcha-response"];
            if (!await Validate(recaptchaSecretKey, recaptchaResponseVal))
            {
                ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
                ViewBag.SiteKey = siteKey;
                return View();
            }

            // Lấy ảnh đại diện (nếu không có thì dùng ảnh mặc định)
            var avatarPath = !string.IsNullOrEmpty(giangVien.Anh)
                ? giangVien.Anh
                : "/images/avt.jpg";

            // Tạo danh sách claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, giangVien.MaGv),
        new Claim(ClaimTypes.Name, giangVien.HoTen),
        new Claim(ClaimTypes.Email, giangVien.Email),
        new Claim("Avatar", avatarPath)
    };

            // ✅ Thêm quyền từ bảng QuyenGV (ClaimTypes.Role)
            foreach (var role in giangVien.QuyenGVs.Select(q => q.Quyen.MaQuyen))
            {
                Console.WriteLine("GÁN ROLE: [" + role + "]");
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            // Tạo identity và đăng nhập
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7),
                    AllowRefresh = true
                });

            return RedirectToAction("Index", "Khoa");
        }

        public async Task<IActionResult> Logout()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["ReturnUrl"] = Request.Path.ToString();
                return RedirectToAction("Index", "Home", new { area = "SV" });
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home", new { area = "SV" });
        }

        // Quên mật khẩu (GET)
        public IActionResult ForgotPassword()
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.SiteKey = siteKey;
            return View();
        }

        // Xử lý Quên Mật khẩu (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            var recaptchaSecretKey = _configuration["ReCaptcha:SecretKey"];
            var recaptchaResponseValue = Request.Form["g-recaptcha-response"];

            // Kiểm tra tính hợp lệ của reCAPTCHA
            var isCaptchaValid = await Validate(recaptchaSecretKey, recaptchaResponseValue);
            if (!isCaptchaValid)
            {
                ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
                ViewBag.SiteKey = siteKey;
                return View();
            }

            // Kiểm tra giảng viên có tồn tại không
            var giangVien = await _context.Giangviens.FirstOrDefaultAsync(gv => gv.Email == email);
            if (giangVien == null)
            {
                ViewBag.SiteKey = siteKey;
                ModelState.AddModelError(string.Empty, "Email không tồn tại trong hệ thống.");
                return View();
            }

            // Tạo token reset mật khẩu và thời gian hết hạn
            string resetToken = Guid.NewGuid().ToString();
            giangVien.ResetToken = resetToken; // Lưu token vào database
            giangVien.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Thời gian hết hạn là 1 giờ

            _context.Update(giangVien);
            await _context.SaveChangesAsync();

            // Tạo đường dẫn reset mật khẩu
            string resetLink = Url.Action("ResetPassword", "Admin", new { token = resetToken }, Request.Scheme);

            // Gửi email cho giảng viên
            await _emailHelper.SendEmailAsync(email, "Đặt lại mật khẩu", $"Giảng viên nhấp vào link sau để đặt lại mật khẩu: <a href='{resetLink}'>{resetLink}</a>");

            // Quay lại trang login
            TempData["SuccessMessage"] = "Một email đã được gửi đến bạn để tạo lại mật khẩu.";
            return RedirectToAction("Login", "Admin");
        }


        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Truy cập không hợp lệ.");
            }

            var giangvien = _context.Giangviens.FirstOrDefault(gv => gv.ResetToken == token);
            if (giangvien == null || giangvien.ResetTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }

            ViewData["Token"] = token;
            ViewBag.SiteKey = _configuration["ReCaptcha:SiteKey"];
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string password, string confirmPassword)
        {
            var siteKey = _configuration["ReCaptcha:SiteKey"];
            var recaptchaSecretKey = _configuration["ReCaptcha:SecretKey"];
            var recaptchaResponseValue = Request.Form["g-recaptcha-response"];

            var isCaptchaValid = await Validate(recaptchaSecretKey, recaptchaResponseValue);
            if (!isCaptchaValid)
            {
                ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
                ViewBag.SiteKey = siteKey;
                ViewData["Token"] = token;
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu và xác nhận mật khẩu không khớp.");
                ViewBag.SiteKey = siteKey;
                ViewData["Token"] = token;
                return View();
            }

            var giangvien = await _context.Giangviens.FirstOrDefaultAsync(gv => gv.ResetToken == token && gv.ResetTokenExpiry > DateTime.UtcNow);
            if (giangvien == null)
            {
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }

            giangvien.Password = password;  // Lưu mật khẩu trực tiếp
            giangvien.ResetToken = null;
            giangvien.ResetTokenExpiry = null;

            _context.Giangviens.Update(giangvien);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Admin");
        }


    }
}
