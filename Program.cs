using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QLSV.Helpers;
using QLSV.Models;
using QLSV.Services;
using QuestPDF.Infrastructure;



var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<HocbasvContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionDB")));



// Cấu hình Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Scheme mặc định
    options.DefaultChallengeScheme = "SinhVienCookie";
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => // Scheme cho Admin
{
    options.LoginPath = "/dang-nhap";
    options.LogoutPath = "/Admin/Logout";
    options.AccessDeniedPath = "/Admin/AccessDenied"; ;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.Name = "Admin.AuthCookie";
    options.Cookie.SameSite = SameSiteMode.Strict;
})
.AddCookie("SinhVienCookie", options => // Scheme cho Sinh viên
{
    options.LoginPath = "/SV/TblSinhvien/LoginSV";
    options.LogoutPath = "/SV/TblSinhvien/LogoutSV";
    //options.AccessDeniedPath = "/SV/TblSinhvien/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.Name = "SinhVien.AuthCookie";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Path = "/SV";
});

// Đăng ký Authorization
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<EmailHelper>();



//đăng ký để xuất pdf
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Phải có UseAuthentication trước UseAuthorization
app.UseAuthentication();
app.UseAuthorization();


// Các route của sinh viên
app.MapControllerRoute(
    name: "SinhVienArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "sinhvien_login",
    pattern: "SV/Dang-nhap-sv",
    defaults: new { area = "SV", controller = "TblSinhvien", action = "LoginSV" });
app.MapControllerRoute(
    name: "detailSV",
    pattern: "SV/Thong-tin-sinh-vien",
    defaults: new { area = "SV", controller = "TblSinhvien", action = "ThongTinSV" }
);

app.MapControllerRoute(
    name: "forgotPassword",
    pattern: "SV/Quen-mat-khau-sv",
    defaults: new { area = "SV", controller = "TblSinhvien", action = "ForgotPasswordSV" }
);

app.MapControllerRoute(
    name: "resetPassword",
    pattern: "SV/Dat-lai-mat-khau-sv",
    defaults: new { area = "SV", controller = "TblSinhvien", action = "ResetPasswordSV" }
);



// Các route của admin
app.MapControllerRoute(
    name: "admin_login",
    pattern: "Dang-nhap",
    defaults: new { controller = "Admin", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

