using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Nganh
{
    public string MaNganh { get; set; } = null!;

    public string TenNganh { get; set; } = null!;

    public string MaKhoa { get; set; } = null!;

    public virtual ICollection<Ctdt> Ctdts { get; set; } = new List<Ctdt>();

    public virtual ICollection<Lop> Lops { get; set; } = new List<Lop>();

    public virtual Khoa MaKhoaNavigation { get; set; } = null!;

    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
}
