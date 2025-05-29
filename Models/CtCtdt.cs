using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLSV.Models;

public partial class CtCtdt
{
    [Required(ErrorMessage = "Mã chương trình đào tạo không được để trống.")]
    public string MaCtdt { get; set; } = null!;

    [Required(ErrorMessage = "Mã năm học không được để trống.")]
    public string MaNh { get; set; } = null!;

    [Required(ErrorMessage = "Mã học kỳ không được để trống.")]
    public string MaHk { get; set; } = null!;

    [Required(ErrorMessage = "Mã môn học không được để trống.")]
    public string MaMh { get; set; } = null!;

    [Required(ErrorMessage = "Mã Nhóm học phần không được để trống.")]
    public string MaNhomHp { get; set; } = null!;

    [Required(ErrorMessage = "Loại môn học không được để trống.")]
    public bool LoaiMh { get; set; }


    // Đảm bảo khởi tạo navigation properties để tránh lỗi NullReferenceException
    public virtual Ctdt? MaCtdtNavigation { get; set; } = null!;
    public virtual Hocky? MaHkNavigation { get; set; } = null!;
    public virtual Monhoc? MaMhNavigation { get; set; } = null!;
    public virtual Namhoc? MaNhNavigation { get; set; } = null!;
    public virtual Nhomhp? MaNhomHpNavigation { get; set; } = null!;


}
