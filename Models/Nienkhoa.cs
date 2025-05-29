using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Nienkhoa
{
    public string MaNk { get; set; } = null!;

    public string TenNk { get; set; } = null!;

    public virtual ICollection<Khht> Khhts { get; set; } = new List<Khht>();

    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
}
