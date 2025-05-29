using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLSV.Models;

public partial class Giangvien
{
    [Required(ErrorMessage = "Mã giảng viên không được để trống.")]
    public string MaGv { get; set; } = null!;

    [Required]
    public bool IsAdmin { get; set; }

    [Required(ErrorMessage = "Họ tên không được để trống.")]
    public string HoTen { get; set; } = null!;

    public string? Anh { get; set; }

    [Required(ErrorMessage = "Giới tính không được để trống.")]
    public bool GioiTinh { get; set; }

    [Required(ErrorMessage = "Ngày sinh không được để trống.")]
    public DateOnly NgaySinh { get; set; }

    public string? QueQuan { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? Sdt { get; set; }

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    public string TenDn { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Mã bộ môn không được để trống.")]
    public string MaBm { get; set; } = null!;

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }


    // Đảm bảo khởi tạo navigation properties để tránh lỗi NullReferenceException
    public virtual Bomon? MaBmNavigation { get; set; } = null;
    public virtual ICollection<QuyenGV> QuyenGVs { get; set; } = new List<QuyenGV>();
}
