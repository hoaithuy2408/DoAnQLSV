using System.Collections.Generic;

namespace QLSV.ViewModel
{
    public class MonHocChuaHoanThanhViewModel
    {
        public string MaNhomHp { get; set; }       
        public string MaMh { get; set; }          
        public string TenMh { get; set; }          
        public int SoTc { get; set; }              
        public string LoaiMh { get; set; }         
        public string TrangThai { get; set; }      
    }

    public class NhomHocPhanViewModel
    {
        public string MaNhomHp { get; set; }       
        public string TenNhomHp { get; set; }      
        public int SoTcBatBuocCan { get; set; }
        public int SoTcTuChonCan { get; set; }
        public int SoTcBatBuocDat { get; set; }
        public int SoTcTuChonDat { get; set; }
        public int SoTcCan { get; set; }           
        public int SoTcDat { get; set; }
        public bool  LoaiMh { get; set; }

        public bool Dat { get; set; }              // Đã đạt yêu cầu nhóm?
        public List<MonHocChuaHoanThanhViewModel> MonHoc { get; set; } = new();
    }

    public class XetTotNghiepViewModel
    {
        // Thông tin sinh viên
        public string MaSv { get; set; }
        public string HoTen { get; set; }
        public string MaCTDT { get; set; }

        // Tiêu chí yêu cầu chung
        public int SoTCYeuCau { get; set; }
        public int SoTC_BatBuoc { get; set; }
        public int SoTC_TuChon { get; set; }
        public double GpaYeuCau { get; set; }

        // Thực tế tích lũy chung
        public int SoTCDaTichLuy { get; set; }
        public int SoTCBatBuocDat { get; set; }
        public int SoTCTuChonDat { get; set; }
        public double GpaTichLuy { get; set; }
        public int SoMonChuaDat { get; set; }

        // Kết quả chi tiết chung
        public bool DatTCTong { get; set; }
        public bool DatTCBatBuoc { get; set; }
        public bool DatTCTuChon { get; set; }
        public bool DatGPA { get; set; }
        public bool DatMon { get; set; }

        // Kết luận tổng thể
        public bool DatTotNghiep { get; set; }

        // Danh sách môn học chưa hoàn thành hoặc chưa đăng ký
        public List<MonHocChuaHoanThanhViewModel> MonChuaHoanThanh { get; set; } = new();

        // Kết quả theo nhóm học phần
        public List<NhomHocPhanViewModel> NhomHocPhan { get; set; } = new();

        // Chi tiết điều kiện theo nhóm (tùy chọn hiển thị thêm)
        public Dictionary<string, bool> DatTheoNhom { get; set; } = new();
    }
}
