namespace QLSV.ViewModel
{
    //phần danh sách môn học thuộc Nhóm HP trong xét tốt ngiệp
    public class MonTheoNhomViewModel
    {
        public string MaCtdt { get; set; } = null!;
        public string MaNhomHp { get; set; } = null!;
        public string TenNhomHp { get; set; } = null!;
        public List<MonHocViewModel> MonHocs { get; set; } = new();
    }

    public class MonHocViewModel
    {
        public string MaMh { get; set; } = null!;
        public string TenMh { get; set; } = null!;
        public int SoTc { get; set; }
    }

}
