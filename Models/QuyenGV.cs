namespace QLSV.Models
{
    public class QuyenGV
    {
        public string MaQuyen { get; set; } = null!;
        public string MaGv { get; set; } = null!;

        public virtual Quyen Quyen { get; set; } = null!;
        public virtual Giangvien Giangvien { get; set; } = null!;
    }
}
