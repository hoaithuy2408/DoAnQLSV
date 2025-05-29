using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Khoa
{
    public string MaKhoa { get; set; } = null!;

    public string TenKhoa { get; set; } = null!;

    public virtual ICollection<Bomon> Bomons { get; set; } = new List<Bomon>();

    public virtual ICollection<Ctdt> Ctdts { get; set; } = new List<Ctdt>();

    public virtual ICollection<Lop> Lops { get; set; } = new List<Lop>();

    public virtual ICollection<Nganh> Nganhs { get; set; } = new List<Nganh>();

    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
}
