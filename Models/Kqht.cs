using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Kqht
{
    public string MaKqht { get; set; } = null!;

    public double? DiemQt { get; set; }

    public double? DiemGk { get; set; }

    public double? DiemCk { get; set; }

    public double? TyLeQt { get; set; }

    public double? TyLeGk { get; set; }

    public double? TyLeCk { get; set; }

    public double? DiemTb { get; set; }

    public string MaSv { get; set; } = null!;

    public string MaMh { get; set; } = null!;

    public string MaNh { get; set; } = null!;

    public string MaHk { get; set; } = null!;

    public virtual Hocky MaHkNavigation { get; set; } = null!;

    public virtual Monhoc MaMhNavigation { get; set; } = null!;

    public virtual Namhoc MaNhNavigation { get; set; } = null!;

    public virtual Sinhvien MaSvNavigation { get; set; } = null!;
}
