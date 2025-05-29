using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class CtKhht
{
    public string MaKhht { get; set; } = null!;

    public string MaMh { get; set; } = null!;

    public string? MaNh { get; set; }

    public string? MaHk { get; set; }

    public bool? LoaiMh { get; set; }
    public string? GhiChu { get; set; }

    public virtual ICollection<CtKhhtSv> CtKhhtSvs { get; set; } = new List<CtKhhtSv>();

    public virtual Khht? MaKhhtNavigation { get; set; }

    public virtual Hocky? MaHkNavigation { get; set; }

    public virtual Monhoc MaMhNavigation { get; set; } = null!;

    public virtual Namhoc? MaNhNavigation { get; set; }
}
