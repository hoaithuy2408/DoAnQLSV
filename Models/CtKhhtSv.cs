using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class CtKhhtSv
{
    public string MaSv { get; set; } = null!;

    public string MaKhht { get; set; } = null!;

    public string MaMh { get; set; } = null!;

    public string MaNh { get; set; } = null!;

    public string MaHk { get; set; } = null!;

    public bool LoaiMh { get; set; }

    public bool XacNhanDk { get; set; }

    public virtual CtKhht CtKhht { get; set; } = null!;
    public virtual Hocky MaHkNavigation { get; set; } = null!;

    public virtual Namhoc MaNhNavigation { get; set; } = null!;
    public virtual Sinhvien MaSvNavigation { get; set; } = null!;
    public virtual Khht? MaKhhtNavigation { get; set; }

    public virtual Monhoc MaMhNavigation { get; set; } = null!;


}
