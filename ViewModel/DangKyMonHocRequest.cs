namespace QLSV.ViewModel
{
    public class DangKyMonHocRequest
    {
        //dành cho đk khht trang sinh viên
        public string MaSv { get; set; }
        public string MaNh { get; set; }    // Năm học
        public string MaHk { get; set; }    // Học kỳ
        public List<CourseSelectionDto> Courses { get; set; } = new List<CourseSelectionDto>();
    }

    public class CourseSelectionDto
    {
        public string Key { get; set; }         // Key dạng "MaKhht_MaMh"
        public bool Selected { get; set; }       
        public string MaNh { get; set; }          
        public string MaHk { get; set; }          
    }


}
