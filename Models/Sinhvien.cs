using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLSV.Models;

public partial class Sinhvien
{
    [Required(ErrorMessage = "Mã sinh viên không được để trống.")]
    public string MaSv { get; set; } = null!;

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

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }


    [Required(ErrorMessage = "Mã khoa không được để trống.")]
    public string MaKhoa { get; set; } = null!;

    [Required(ErrorMessage = "Mã ngành không được để trống.")]
    public string MaNganh { get; set; } = null!;

    [Required(ErrorMessage = "Mã lớp không được để trống.")]
    public string MaLop { get; set; } = null!;

    [Required(ErrorMessage = "Mã niên khóa không được để trống.")]
    public string MaNk { get; set; } = null!;

    [Required(ErrorMessage = "Mã CTDT không được để trống.")]
    public string MaCtdt { get; set; } = null!;

    public string? DiaChi { get; set; }
    public string? DanToc { get; set; }

    public string? Cccd { get; set; }


    // Đảm bảo khởi tạo navigation properties để tránh lỗi NullReferenceException
    public virtual Ctdt? MaCtdtNavigation { get; set; } = null!;
    public virtual Khoa? MaKhoaNavigation { get; set; } = null;
    public virtual Lop? MaLopNavigation { get; set; } = null;
    public virtual Nganh? MaNganhNavigation { get; set; } = null;
    public virtual Nienkhoa? MaNkNavigation { get; set; } = null!;

    public virtual ICollection<Kqht> Kqhts { get; set; } = new List<Kqht>();
    public virtual ICollection<CtKhhtSv> CtKhhtSvs { get; set; } = new List<CtKhhtSv>();



}

