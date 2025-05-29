using System;
using System.Collections.Generic;

namespace QLSV.Models;

public partial class Thoigiandangky
{
    public string MaNh { get; set; } = null!;

    public string MaHk { get; set; } = null!;

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public bool ChoPhepDangKy { get; set; }

    public virtual Hocky? MaHkNavigation { get; set; } = null!;

    public virtual Namhoc? MaNhNavigation { get; set; } = null!;
}
