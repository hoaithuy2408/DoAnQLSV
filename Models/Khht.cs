using System.ComponentModel.DataAnnotations;

namespace QLSV.Models;

public partial class Khht
{
    [Required(ErrorMessage = "Mã KHHT không được để trống.")]
    public string MaKhht { get; set; } = null!;

    [Required(ErrorMessage = "Tên KHHT không được để trống.")]
    public string TenKhht { get; set; } = null!;

    [Required(ErrorMessage = "Tổng số TC không được để trống.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số tín chỉ tổng phải lớn hơn 0.")]
    public int SoTctong { get; set; }

    [Required(ErrorMessage = "Tổng số TC không được để trống.")]
    [Range(0, int.MaxValue, ErrorMessage = "Số tín chỉ tự chọn không hợp lệ.")]
    public int SoTctuChon { get; set; }

    [Required(ErrorMessage = "Tổng số TC không được để trống.")]
    [Range(0, int.MaxValue, ErrorMessage = "Số tín chỉ bắt buộc không hợp lệ.")]
    public int SoTcbatBuoc { get; set; }

    [Required(ErrorMessage = "Năm học không được để trống.")]
    public string MaNh { get; set; } = null!;

    [Required(ErrorMessage = "Học kỳ không được để trống.")]
    public string MaHk { get; set; } = null!;

    [Required(ErrorMessage = "Chương trình đào tạo không được để trống.")]
    public string MaCtdt { get; set; } = null!;

    [Required(ErrorMessage = "Niên khóa không được để trống.")]
    public string MaNk { get; set; } = null!;

    public virtual Ctdt? MaCtdtNavigation { get; set; } = null!;
    public virtual Hocky? MaHkNavigation { get; set; } = null!;
    public virtual Namhoc? MaNhNavigation { get; set; } = null!;
    public virtual Nienkhoa? MaNkNavigation { get; set; } = null!;

}
