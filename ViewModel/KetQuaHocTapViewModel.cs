namespace QLSV.ViewModel
{
    public class KetQuaHocTapViewModel
    {
        public string MaSv{ get; set; }
        public string HoTen { get; set; }

        public string MaMh { get; set; }
        public string TenMh { get; set; }
        public string MaNh { get; set; }
        public string TenNh { get; set; }
        public string MaHk { get; set; }
        public int HocKy { get; set; }
        public int SoTc { get; set; }      

        public double? DiemQt { get; set; }
        public double? DiemGk { get; set; }
        public double? DiemCk { get; set; }
        public double DiemTb =>
            ((DiemQt ?? 0) * TyLeQt
           + (DiemGk ?? 0) * TyLeGk
           + (DiemCk ?? 0) * TyLeCk) / 100.0;

        public double TyLeQt { get; set; }
        public double TyLeGk { get; set; }
        public double TyLeCk { get; set; }
    }
}
