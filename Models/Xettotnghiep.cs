using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Xettotnghiep
{
    public string MaCtdt { get; set; } = null!;

    public int SoTcTong { get; set; }

    public int SoTcTuChon { get; set; }

    public int SoTcBatBuoc { get; set; }

    public decimal GpatoiThieu { get; set; }

    public virtual Ctdt? MaCtdtNavigation { get; set; } = null!;
}
