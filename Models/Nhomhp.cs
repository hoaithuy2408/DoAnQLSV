using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Nhomhp
{
    public string MaNhomHp { get; set; } = null!;

    public string TenNhomHp { get; set; } = null!;

    public virtual ICollection<CtCtdt> CtCtdts { get; set; } = new List<CtCtdt>();

    public virtual ICollection<XettotnghiepNhom> XettotnghiepNhoms { get; set; } = new List<XettotnghiepNhom>();
}
