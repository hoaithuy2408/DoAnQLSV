using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Quyen
{
    public string MaQuyen { get; set; } = null!;

    public string TenQuyen { get; set; } = null!;

    public virtual ICollection<QuyenGV> QuyenGVs { get; set; } = new List<QuyenGV>();

}
