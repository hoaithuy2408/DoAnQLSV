using System;
using System.Collections.Generic;

namespace QLSV.Models;

using System.ComponentModel.DataAnnotations;

public partial class Ctdt
{
    [Required(ErrorMessage = "Mã chương trình đào tạo không được để trống.")]
    public string MaCtdt { get; set; } = null!;

    [Required(ErrorMessage = "Tên chương trình đào tạo không được để trống.")]
    public string TenCtdt { get; set; } = null!;

    [Required(ErrorMessage = "Hình thức đào tạo không được để trống.")]
    public bool HinhThucDt { get; set; }  // Không nên dùng bool?, đổi sang bool

    [Required(ErrorMessage = "Tổng số tín chỉ không được để trống.")]
    [Range(1, int.MaxValue, ErrorMessage = "Tổng số tín chỉ phải lớn hơn 0.")]
    public int TongSoTc { get; set; }  // Không nên dùng int?, đổi sang int

    [Required(ErrorMessage = "Mã khoa không được để trống.")]
    public string MaKhoa { get; set; } = null!;

    [Required(ErrorMessage = "Mã ngành không được để trống.")]
    public string MaNganh { get; set; } = null!;


    // Đảm bảo khởi tạo navigation properties để tránh lỗi NullReferenceException
    public virtual Khoa? MaKhoaNavigation { get; set; } = null;
    public virtual Nganh? MaNganhNavigation { get; set; } = null;

    public virtual ICollection<CtCtdt> CtCtdts { get; set; } = new List<CtCtdt>();
    public virtual ICollection<Khht> Khhts { get; set; } = new List<Khht>();
    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
    public virtual Xettotnghiep? Xettotnghiep { get; set; }
    public virtual ICollection<XettotnghiepNhom> XettotnghiepNhoms { get; set; } = new List<XettotnghiepNhom>();


}
