using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class KhhtSv
{
    public string MaKhhtSv { get; set; } = null!;

    public string MaSv { get; set; } = null!;

    public string MaKhht { get; set; } = null!;

    public string MaMh { get; set; } = null!;

    public bool XacNhanDk { get; set; }

    public virtual Khht MaKhhtNavigation { get; set; } = null!;

    public virtual Monhoc MaMhNavigation { get; set; } = null!;

    public virtual Sinhvien MaSvNavigation { get; set; } = null!;
}
