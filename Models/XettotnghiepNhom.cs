using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class XettotnghiepNhom
{
    public string MaCtdt { get; set; } = null!;

    public string MaNhomHp { get; set; } = null!;

    public int SoTcTuChon { get; set; }

    public int SoTcBatBuoc { get; set; }
    public virtual Ctdt? MaCtdtNavigation { get; set; } = null!;

    public virtual Nhomhp? MaNhomHpNavigation { get; set; } = null!;
}
