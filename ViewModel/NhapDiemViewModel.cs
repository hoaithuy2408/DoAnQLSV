using System.ComponentModel.DataAnnotations;

namespace QLSV.ViewModel
{
    public class NhapDiemViewModel
    {
        public string MaKqht { get; set; }    
        public string MaSv { get; set; }
        public string HoTen { get; set; }

        public string MaMh { get; set; }
        public string MaLop { get; set; }
        public string MaNh { get; set; }
        public string MaHk { get; set; }
        public double TyLeQt { get; set; }
        public double TyLeGk { get; set; }
        public double TyLeCk { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm Quá trình phải trong khoảng 0–10")]
        public double? DiemQt { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm Giữa kỳ phải trong khoảng 0–10")]
        public double? DiemGk { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm Cuối kỳ phải trong khoảng 0–10")]
        public double? DiemCk { get; set; }


        // Điểm tb tự tính
        public double? DiemTb =>
            ((DiemQt ?? 0) * TyLeQt
            + (DiemGk ?? 0) * TyLeGk
            + (DiemCk ?? 0) * TyLeCk)
            / 100.0;

    }
}
