using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Lop
{
    public string MaLop { get; set; } = null!;

    public string TenLop { get; set; } = null!;

    public string MaKhoa { get; set; } = null!;

    public string MaNganh { get; set; } = null!;

    public virtual Khoa MaKhoaNavigation { get; set; } = null!;

    public virtual Nganh MaNganhNavigation { get; set; } = null!;

    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
}
