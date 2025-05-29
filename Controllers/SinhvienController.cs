using HTMLQuestPDF.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSV.Models;
using QLSV.Services;
using QLSV.ViewModel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace QLSV.Controllers
{
    [Authorize(AuthenticationSchemes = "Cookies")]
    public class SinhvienController : Controller
    {
        private readonly HocbasvContext _context;
        private readonly IViewRenderService _viewRenderService;

        public SinhvienController(HocbasvContext context, IViewRenderService viewRenderService)
        {
            _context = context;
            _viewRenderService = viewRenderService;
            ;
        }

        // Hiển thị trang chi tiết KHHT của sinh viên (VIEW FULL)
        public IActionResult ChiTietKHHTSinhVien(string maSv, string? maNh, string? maHk, bool? dangKyMode)
        {
            if (string.IsNullOrEmpty(maSv))
                return BadRequest("Thiếu mã sinh viên");

            var sinhvien = _context.Sinhviens
                .Include(s => s.MaNkNavigation)
                .Include(s => s.MaCtdtNavigation)
                .FirstOrDefault(s => s.MaSv == maSv);

            if (sinhvien == null)
                return NotFound("Không tìm thấy sinh viên");

            // Xác định các năm học hợp lệ theo niên khóa
            int namBatDauNk = int.Parse(sinhvien.MaNkNavigation.TenNk.Split('-')[0]);
            int namKetThucNk = int.Parse(sinhvien.MaNkNavigation.TenNk.Split('-')[1]);
            var allNamhocs = _context.Namhocs.ToList();
            var namHocs = allNamhocs
                .Where(nh =>
                {
                    int namBatDauNh = int.Parse(nh.TenNh.Split('-')[0]);
                    return namBatDauNh >= namBatDauNk && namBatDauNh < namKetThucNk;
                })
                .OrderBy(nh => nh.MaNh)
                .ToList();

            // Lấy tất cả KHHT phù hợp
            var khhts = _context.Khhts
                .Where(k => k.MaCtdt == sinhvien.MaCtdt && k.MaNk == sinhvien.MaNk)
                .ToList();
            if (!khhts.Any())
                return NotFound("Không có KHHT phù hợp");

            var khhtIds = khhts.Select(k => k.MaKhht).ToList();

            // Lấy các môn học trong KHHT
            var dsMonHoc = _context.CtKhhts
                .Include(ct => ct.MaMhNavigation)
                .Include(ct => ct.MaKhhtNavigation)
                .Where(ct => khhtIds.Contains(ct.MaKhht))
                .ToList();

            // Map MaKhht với MaNh/MaHk
            var khhtMap = khhts.ToDictionary(
                k => k.MaKhht,
                k => new { k.MaNh, k.MaHk }
            );

            // Tạo CtKhhtSv nếu chưa có
            foreach (var item in dsMonHoc)
            {
                var info = khhtMap[item.MaKhht];

                bool exists = _context.CtKhhtSvs.Any(ct =>
                    ct.MaSv == maSv &&
                    ct.MaKhht == item.MaKhht &&
                    ct.MaMh == item.MaMh
                );

                if (!exists)
                {
                    _context.CtKhhtSvs.Add(new CtKhhtSv
                    {
                        MaSv = maSv,
                        MaKhht = item.MaKhht,
                        MaMh = item.MaMh,
                        MaNh = info.MaNh,
                        MaHk = info.MaHk,
                        LoaiMh = item.LoaiMh ?? false,
                        XacNhanDk = false
                    });
                }
            }
            _context.SaveChanges();

            // Truy vấn các bản ghi cho view
            var query = _context.CtKhhtSvs
                .Include(ct => ct.MaMhNavigation)
                .Include(ct => ct.MaKhhtNavigation)
                .Where(ct => ct.MaSv == maSv && khhtIds.Contains(ct.MaKhht));

            if (!string.IsNullOrEmpty(maNh))
                query = query.Where(ct => ct.MaKhhtNavigation.MaNh == maNh);

            if (!string.IsNullOrEmpty(maHk))
                query = query.Where(ct => ct.MaKhhtNavigation.MaHk == maHk);

            var result = query
                .OrderBy(ct => ct.MaKhhtNavigation.MaNh)
                .ThenBy(ct => ct.MaKhhtNavigation.MaHk)
                .ToList();

            // Dropdowns
            ViewBag.NamHocs = _context.Namhocs
                .OrderBy(n => n.TenNh)
                .Select(n => new SelectListItem
                {
                    Value = n.MaNh,
                    Text = n.TenNh,
                    Selected = (n.MaNh == maNh)
                }).ToList();

            ViewBag.HocKys = _context.Hockies
                .OrderBy(h => h.MaHk)
                .Select(h => new SelectListItem
                {
                    Value = h.MaHk,
                    Text = h.MaHk,
                    Selected = (h.MaHk == maHk)
                }).ToList();

            ViewBag.DangKyMode = dangKyMode ?? false;
            ViewBag.MaSv = maSv;
            ViewBag.MaNk = sinhvien.MaNk;
            ViewBag.CurrentNamHoc = maNh;
            ViewBag.CurrentHocKy = maHk;

            // Truyền thời gian đăng ký cho view partial (nếu cần so sánh phía client)
            ViewBag.ThoiGianDangKy = _context.Thoigiandangkies.ToList();

            return View(result);
        }

        // PartialView load lại danh sách môn học 
        public IActionResult ChiTietKHHTSinhVienPartial(string maSv, string? maNh, string? maHk, bool dangKyMode)
        {
            var qry = _context.CtKhhtSvs
                .Include(x => x.MaMhNavigation)
                .Include(x => x.MaKhhtNavigation)
                .Where(x => x.MaSv == maSv);

            if (!string.IsNullOrEmpty(maNh))
                qry = qry.Where(x => x.MaNh == maNh);
            if (!string.IsNullOrEmpty(maHk))
                qry = qry.Where(x => x.MaHk == maHk);

            var list = qry.ToList();

            // Drop down
            ViewBag.NamHocs = _context.Namhocs
                .OrderBy(n => n.TenNh)
                .Select(n => new SelectListItem
                {
                    Value = n.MaNh,
                    Text = n.TenNh,
                    Selected = (n.MaNh == maNh)
                }).ToList();

            ViewBag.HocKys = _context.Hockies
                .OrderBy(h => h.MaHk)
                .Select(h => new SelectListItem
                {
                    Value = h.MaHk,
                    Text = h.MaHk,
                    Selected = (h.MaHk == maHk)
                }).ToList();

            ViewBag.DangKyMode = dangKyMode;
            ViewBag.ThoiGianDangKy = _context.Thoigiandangkies.ToList();

            return PartialView("_DanhSachMonHocPartial", list);
        }


        // Xử lý đăng ký môn học (AJAX)
        [HttpPost]
        public IActionResult DangKyMonHoc([FromBody] DangKyMonHocRequest data)
        {
            if (string.IsNullOrEmpty(data.MaSv))
                return BadRequest("Thiếu mã sinh viên");

            var regs = _context.CtKhhtSvs
                .Where(ct => ct.MaSv == data.MaSv)
                .ToList();

            var warnings = new List<string>();
            var now = DateTime.Now;

            foreach (var course in data.Courses)
            {
                var parts = course.Key.Split('_');
                if (parts.Length != 2) continue;
                var maKhht = parts[0];
                var maMh = parts[1];

                var reg = regs.FirstOrDefault(r => r.MaKhht == maKhht && r.MaMh == maMh);
                if (reg == null) continue;

                bool oldSelected = reg.XacNhanDk;
                bool newSelected = course.Selected;

                // Lấy thông tin kỳ đăng ký từ request (theo năm/học kỳ được gởi lên)
                var period = _context.Thoigiandangkies
                    .SingleOrDefault(t => t.MaNh == course.MaNh && t.MaHk == course.MaHk);

                string? reason = null;

                // ----- Kiểm tra cho ĐĂNG KÝ MỚI -----
                if (!oldSelected && newSelected)
                {
                    if (period == null)
                        reason = "Chưa cấu hình thời gian đăng ký";
                    else if (!period.ChoPhepDangKy)
                        reason = "Kỳ đăng ký đã đóng";
                    else if (now < period.NgayBatDau)
                        reason = $"Chưa đến ngày bắt đầu ({period.NgayBatDau:dd/MM/yyyy})";
                    else if (now > period.NgayKetThuc)
                        reason = $"Đã hết hạn ({period.NgayKetThuc:dd/MM/yyyy})";

                    if (reason != null)
                    {
                        warnings.Add($"{reg.MaMh}: {reason}");
                        continue; // không thực hiện đăng ký
                    }

                    // qua hết warning, thực hiện đăng ký
                    reg.XacNhanDk = true;
                    reg.MaNh = course.MaNh;
                    reg.MaHk = course.MaHk;
                }
                // ----- Kiểm tra cho HỦY ĐĂNG KÝ -----
                else if (oldSelected && !newSelected)
                {
                    if (period == null)
                        reason = "Chưa cấu hình thời gian đăng ký";
                    else if (!period.ChoPhepDangKy)
                        reason = "Kỳ đăng ký đã đóng";
                    else if (now < period.NgayBatDau)
                        reason = $"Chưa đến ngày bắt đầu ({period.NgayBatDau:dd/MM/yyyy})";
                    else if (now > period.NgayKetThuc)
                        reason = $"Đã hết hạn ({period.NgayKetThuc:dd/MM/yyyy})";

                    if (reason != null)
                    {
                        warnings.Add($"{reg.MaMh}: Không thể hủy đăng ký – {reason}");
                        continue; // không thực hiện hủy
                    }

                    // Xóa kết quả học tập nếu đã nhập
                    var existingResults = _context.Kqhts
                        .Where(k => k.MaSv == data.MaSv
                                 && k.MaMh == maMh
                                 && k.MaNh == reg.MaNh
                                 && k.MaHk == reg.MaHk)
                        .ToList();
                    if (existingResults.Any())
                        _context.Kqhts.RemoveRange(existingResults);

                    // Thực hiện hủy
                    reg.XacNhanDk = false;
                }
                // Nếu không thay đổi (chọn lại trạng thái cũ) thì bỏ qua
            }

            _context.SaveChanges();

            // Trả về warning nếu có
            if (warnings.Any())
            {
                return Json(new
                {
                    success = true,
                    message = "Cập nhật thành công, nhưng có một số môn chưa được xử lý:",
                    warnings
                });
            }
            else
            {
                return Json(new { success = true, message = "Cập nhật đăng ký môn học thành công!" });
            }
        }

        private async Task<List<KetQuaHocTapViewModel>> GetKetQuaHocTapAsync(string maSv)
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
                            .Include(ct => ct.MaKhhtNavigation.MaNhNavigation)
                            .Include(ct => ct.MaKhhtNavigation.MaHkNavigation)
                        where ct.MaSv == maSv && ct.XacNhanDk == true
                        join kq in _context.Kqhts.Where(k => k.MaSv == maSv)
                            on new { ct.MaSv, ct.MaMh, ct.MaNh, ct.MaHk }
                            equals new { kq.MaSv, kq.MaMh, kq.MaNh, kq.MaHk }
                            into gj
                        from kq in gj.DefaultIfEmpty()
                        orderby ct.MaKhhtNavigation.MaNh, ct.MaKhhtNavigation.MaHk, ct.MaMh
                        select new KetQuaHocTapViewModel
                        {
                            MaSv = maSv,
                            HoTen = ct.MaSvNavigation.HoTen,
                            MaMh = ct.MaMh,
                            TenMh = ct.MaMhNavigation.TenMh,
                            MaNh = ct.MaNh,
                            TenNh = _context.Namhocs.Where(n => n.MaNh == ct.MaNh).Select(n => n.TenNh).FirstOrDefault(),
                            MaHk = ct.MaHk,
                            HocKy = _context.Hockies.Where(h => h.MaHk == ct.MaHk).Select(h => h.TenHk).FirstOrDefault(),
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

        [HttpGet("KetQuaHocTapSinhVien")]
        public async Task<IActionResult> KetQuaHocTapSinhVien(string maSv)
        {
            if (string.IsNullOrEmpty(maSv))
                return BadRequest("Thiếu mã sinh viên.");

            var vmList = await GetKetQuaHocTapAsync(maSv);
            return PartialView("_KetQuaHocTapPartial", vmList);
        }


        [HttpGet("XuatKetQuaHocTapPdf")]
        public async Task<IActionResult> XuatKetQuaHocTapPdf(string maSv)
        {
            if (string.IsNullOrWhiteSpace(maSv))
                return BadRequest("Thiếu mã sinh viên.");

            // 1. Lấy dữ liệu
            var vmList = await GetKetQuaHocTapAsync(maSv);

            // 2. Render Razor thành HTML (đặt ViewData flag để ẩn nút, thêm CSS in)
            ViewData["IsExportPdf"] = true;
            string html = await _viewRenderService
                            .RenderViewAsync("_KetQuaHocTapPartial", vmList);

            // 3. Dùng QuestPDF + HTMLToQPDF
            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20, Unit.Millimetre);          
                    page.DefaultTextStyle(t => t.FontSize(11));

                    /* Header */
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
                    /* Nội dung HTML */
                    page.Content().PaddingVertical(5).Column(col =>
                    {
                        col.Item().AlignCenter().Text("Bảng ghi điểm học phần").Bold().FontSize(13).Underline();

                        col.Item().PaddingTop(10); // khoảng trắng trước nội dung HTML thực tế

                        col.Item().HTML(h =>
                        {
                            h.SetHtml(html);
                            h.SetTextStyleForHtmlElement("th", TextStyle.Default.FontSize(11).Bold());
                            h.SetTextStyleForHtmlElement("td", TextStyle.Default.FontSize(11));
                            h.SetTextStyleForHtmlElement("h5", TextStyle.Default.FontSize(12).Bold());
                        });
                        col.Item().Height(20); // khoảng trắng 20pt

                    });

                    
                    /* Footer số trang */
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();


            // 4. Trả file
            return File(pdfBytes, "application/pdf", $"KetQuaHocTap_{maSv}.pdf");
        }
        
        [HttpGet]
        public async Task<IActionResult> XetTotNghiep(string maSv)
        {
            if (string.IsNullOrEmpty(maSv))
                return BadRequest("Thiếu mã sinh viên");

            var sv = await _context.Sinhviens
                .Where(s => s.MaSv == maSv)
                .Select(s => new { s.MaSv, s.HoTen, s.MaCtdt })
                .FirstOrDefaultAsync();
            if (sv == null)
                return NotFound("Không tìm thấy sinh viên");

            string maCtDt = sv.MaCtdt;

            var tc = await _context.Xettotnghieps
                .FirstOrDefaultAsync(x => x.MaCtdt == maCtDt);
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


            return PartialView("_XetTotNghiepPartial", vm);
        }



        public async Task<IActionResult> Index(string searchTerm, string sortColumn, string sortOrder, int page = 1, int pageSize = 8)
        {
            var query = _context.Sinhviens
                .Include(s => s.MaKhoaNavigation)
                .Include(s => s.MaLopNavigation)
                .Include(s => s.MaNganhNavigation)
                .Include(s => s.MaNkNavigation)
                .AsQueryable();

            //  Tìm kiếm theo Họ tên hoặc Email
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.HoTen.Contains(searchTerm) || s.Email.Contains(searchTerm));
            }

            //  Sắp xếp dữ liệu
            switch (sortColumn)
            {
                case "MaSv":
                    query = sortOrder == "asc" ? query.OrderBy(s => s.MaSv) : query.OrderByDescending(s => s.MaSv);
                    break;
                case "HoTen":
                    query = sortOrder == "asc" ? query.OrderBy(s => s.HoTen) : query.OrderByDescending(s => s.HoTen);
                    break;
                case "Email":
                    query = sortOrder == "asc" ? query.OrderBy(s => s.Email) : query.OrderByDescending(s => s.Email);
                    break;
                case "MaKhoa":
                    query = sortOrder == "asc" ? query.OrderBy(s => s.MaKhoa) : query.OrderByDescending(s => s.MaKhoa);
                    break;
                case "MaNganh":
                    query = sortOrder == "asc" ? query.OrderBy(s => s.MaNganh) : query.OrderByDescending(s => s.MaNganh);
                    break;
                case "MaLop":
                    query = sortOrder == "asc" ? query.OrderBy(s => s.MaLop) : query.OrderByDescending(s => s.MaLop);
                    break;
                default:
                    query = query.OrderBy(s => s.MaSv); // Mặc định sắp xếp theo Mã sinh viên
                    break;
            }

            //  Tổng số bản ghi
            int totalItems = await query.CountAsync();

            //  Phân trang
            var sinhviens = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Truyền dữ liệu vào ViewData
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SortColumn"] = sortColumn;
            ViewData["SortOrder"] = sortOrder;
            ViewBag.SearchTerm = searchTerm;

            return View(sinhviens);
        }


        // GET: Sinhvien/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhvien = await _context.Sinhviens
                .Include(s => s.MaKhoaNavigation)
                .Include(s => s.MaLopNavigation)
                .Include(s => s.MaNganhNavigation)
                .Include(s => s.MaNkNavigation)
                .Include(s => s.MaCtdtNavigation)
                .FirstOrDefaultAsync(m => m.MaSv == id);
            if (sinhvien == null)
            {
                return NotFound();
            }

            return View(sinhvien);
        }

        // GET: Sinhvien/Create
        [Authorize(Roles = "Them")]
        public IActionResult Create()
        {
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa");
            ViewData["MaNganh"] = new SelectList(_context.Nganhs, "MaNganh", "TenNganh");
            ViewData["MaLop"] = new SelectList(_context.Lops, "MaLop", "TenLop");
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "TenCtdt");
            ViewData["MaNk"] = new SelectList(_context.Nienkhoas, "MaNk", "TenNk");
            return View();
        }

        // POST: Sinhviens/Create
        [HttpPost]
        [Authorize(Roles = "Them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSv,HoTen,GioiTinh,NgaySinh,QueQuan,Sdt,Email,TenDn,Password,MaKhoa,MaNganh,MaLop,MaNk,MaCtdt,Cccd,DiaChi,DanToc")] Sinhvien sinhvien, IFormFile Anh)
        {
            // Kiểm tra trùng mã sinh viên
            bool isExist = await _context.Sinhviens.AnyAsync(s => s.MaSv == sinhvien.MaSv);
            if (isExist)
            {
                ModelState.AddModelError("MaSv", "Mã sinh viên đã tồn tại.");
            }

            // Kiểm tra trùng Email
            bool isEmailExist = await _context.Sinhviens.AnyAsync(s => s.Email == sinhvien.Email);
            if (isEmailExist)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
            }

            // Xử lý ảnh đại diện
            if (Anh == null || Anh.Length == 0)
            {
                sinhvien.Anh = "/images/avt.jpg"; // Ảnh mặc định
            }
            else
            {
                var fileName = Path.GetFileName(Anh.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                if (!System.IO.File.Exists(uploadPath))
                {
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await Anh.CopyToAsync(stream);
                    }
                }

                sinhvien.Anh = "/images/" + fileName;
            }

            if (ModelState.IsValid)
            {
                _context.Add(sinhvien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Load lại SelectList nếu có lỗi
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", sinhvien.MaKhoa);
            ViewData["MaNganh"] = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", sinhvien.MaNganh);
            ViewData["MaLop"] = new SelectList(_context.Lops, "MaLop", "TenLop", sinhvien.MaLop);
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "TenCtdt", sinhvien.MaCtdt);
            ViewData["MaNk"] = new SelectList(_context.Nienkhoas, "MaNk", "TenNk", sinhvien.MaNk);

            return View(sinhvien);
        }


        // GET: Sinhvien/Edit/5
        [Authorize(Roles = "Sua")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhvien = await _context.Sinhviens.FindAsync(id);
            if (sinhvien == null)
            {
                return NotFound();
            }
            ViewData["MaKhoa"] = new SelectList(_context.Khoas, "MaKhoa", "TenKhoa", sinhvien.MaKhoa);
            ViewData["MaLop"] = new SelectList(_context.Lops, "MaLop", "TenLop", sinhvien.MaLop);
            ViewData["MaNganh"] = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", sinhvien.MaNganh);
            ViewData["MaCtdt"] = new SelectList(_context.Ctdts, "MaCtdt", "TenCtdt", sinhvien.MaCtdt);
            ViewData["MaNk"] = new SelectList(_context.Nienkhoas, "MaNk", "TenNk", sinhvien.MaNk);
            return View(sinhvien);
        }

        // POST: Sinhvien/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaSv,HoTen,GioiTinh,NgaySinh,QueQuan,Sdt,Email,TenDn,Password,MaKhoa,MaNganh,MaLop,MaNk,MaCtdt,Cccd,DiaChi,DanToc")] Sinhvien sinhvien, IFormFile? Anh)
        {
            if (id != sinhvien.MaSv)
            {
                return NotFound();
            }

            var existingSinhvien = await _context.Sinhviens.FirstOrDefaultAsync(s => s.MaSv == id);
            if (existingSinhvien == null)
            {
                return NotFound();
            }

            sinhvien.QueQuan = string.IsNullOrWhiteSpace(sinhvien.QueQuan) ? null : sinhvien.QueQuan;
            sinhvien.Sdt = string.IsNullOrWhiteSpace(sinhvien.Sdt) ? null : sinhvien.Sdt;

            if (Anh != null && Anh.Length > 0)
            {
                var fileExtension = Path.GetExtension(Anh.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Anh", "Chỉ chấp nhận định dạng ảnh JPG, JPEG, PNG, GIF.");
                    return View(sinhvien);
                }

                var fileName = Path.GetFileName(Anh.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                if (!string.IsNullOrEmpty(existingSinhvien.Anh) && existingSinhvien.Anh != "/images/avt.jpg")
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingSinhvien.Anh.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }

                existingSinhvien.Anh = "/images/" + fileName;
            }

            existingSinhvien.HoTen = sinhvien.HoTen;
            existingSinhvien.GioiTinh = sinhvien.GioiTinh;
            existingSinhvien.NgaySinh = sinhvien.NgaySinh;
            existingSinhvien.QueQuan = sinhvien.QueQuan;
            existingSinhvien.Sdt = sinhvien.Sdt;
            existingSinhvien.Email = sinhvien.Email;
            existingSinhvien.TenDn = sinhvien.TenDn;
            existingSinhvien.Password = sinhvien.Password;
            existingSinhvien.MaKhoa = sinhvien.MaKhoa;
            existingSinhvien.MaNganh = sinhvien.MaNganh;
            existingSinhvien.MaLop = sinhvien.MaLop;
            existingSinhvien.MaNk = sinhvien.MaNk;
            existingSinhvien.MaCtdt = sinhvien.MaCtdt;

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SinhvienExists(sinhvien.MaSv))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(sinhvien);
        }


        // GET: Sinhvien/Delete/5
        [Authorize(Roles = "Xoa")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhvien = await _context.Sinhviens
                .Include(s => s.MaKhoaNavigation)
                .Include(s => s.MaLopNavigation)
                .Include(s => s.MaNganhNavigation)
                .Include(s => s.MaNkNavigation)
                .FirstOrDefaultAsync(m => m.MaSv == id);
            if (sinhvien == null)
            {
                return NotFound();
            }

            return View(sinhvien);
        }

        // POST: Sinhvien/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Load cả navigation properties để kiểm tra
            var sinhvien = await _context.Sinhviens
                 .Include(c => c.MaLopNavigation)
                .Include(s => s.Kqhts)
                .Include(s => s.CtKhhtSvs)
                .FirstOrDefaultAsync(s => s.MaSv == id);

            if (sinhvien == null)
                return NotFound();

            // Kiểm tra xem có bản ghi con không
            int countKqht = sinhvien.Kqhts?.Count() ?? 0;
            int countCtKhhtSv = sinhvien.CtKhhtSvs?.Count() ?? 0;

            if (countKqht + countCtKhhtSv > 0)
            {
                // Đưa thông báo về View
                ViewBag.DeleteError = $"Không thể xóa vì còn {countKqht} kết quả môn học và {countCtKhhtSv} chi tiết KHHT liên quan.";
                return View(sinhvien);
            }

            // Nếu không có bản ghi con, cho phép xóa
            _context.Sinhviens.Remove(sinhvien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SinhvienExists(string id)
        {
            return _context.Sinhviens.Any(e => e.MaSv == id);
        }
    }
}
