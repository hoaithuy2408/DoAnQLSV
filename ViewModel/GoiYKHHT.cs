namespace QLSV.ViewModel
{
    public class GoiYKHHT
    {
        public string MaKhht { get; set; }

        // Môn trong kỳ hiện tại chưa đăng ký
        public List<CourseDto> CurrentNotRegistered { get; set; }

        // Môn trượt cùng kỳ trước
        public List<CourseDto> PrevFailed { get; set; }

        // Môn cùng kỳ năm trước chưa đăng ký
        public List<CourseDto> PrevYearNotRegistered { get; set; }
    }

    public class CourseDto
    {
        public string MaMh { get; set; }
        public string TenMh { get; set; }
        public bool? LoaiMh { get; set; }

        public int SoTc { get; set; }
        public double? Grade { get; set; }    // Điểm TB (với PrevFailed)
    }


}
