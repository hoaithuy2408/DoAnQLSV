using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Monhoc
{
    public string MaMh { get; set; } = null!;

    public string TenMh { get; set; } = null!;

    public int SoTc { get; set; }

    public string? MaBm { get; set; }

    public virtual ICollection<CtCtdt> CtCtdts { get; set; } = new List<CtCtdt>();

    public virtual ICollection<CtKhht> CtKhhts { get; set; } = new List<CtKhht>();

    public virtual ICollection<Kqht> Kqhts { get; set; } = new List<Kqht>();

    public virtual Bomon? MaBmNavigation { get; set; }
}
