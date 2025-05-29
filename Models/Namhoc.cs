using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Namhoc
{
    public string MaNh { get; set; } = null!;

    public string TenNh { get; set; } = null!;

    public virtual ICollection<CtCtdt> CtCtdts { get; set; } = new List<CtCtdt>();

    public virtual ICollection<CtKhht> CtKhhts { get; set; } = new List<CtKhht>();

    public virtual ICollection<Khht> Khhts { get; set; } = new List<Khht>();

    public virtual ICollection<Kqht> Kqhts { get; set; } = new List<Kqht>();
    public virtual ICollection<CtKhhtSv> CtKhhtSvs { get; set; } = new List<CtKhhtSv>();

    public virtual ICollection<Thoigiandangky> Thoigiandangkies { get; set; } = new List<Thoigiandangky>();

}
