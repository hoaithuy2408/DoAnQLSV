using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLSV.Models;

public partial class Bomon
{
    [Required(ErrorMessage = "Mã bộ môn không được để trống.")]
    public string MaBm { get; set; } = null!;

    [Required(ErrorMessage = "Tên bộ môn không được để trống.")]
    public string TenBm { get; set; } = null!;

    [Required(ErrorMessage = "Mã khoa không được để trống.")]
    public string MaKhoa { get; set; } = null!;

    public virtual Khoa? MaKhoaNavigation { get; set; } = null!;

    public virtual ICollection<Giangvien> Giangviens { get; set; } = new List<Giangvien>();
    public virtual ICollection<Monhoc> Monhocs { get; set; } = new List<Monhoc>();
}