using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace QLSV.ViewModels
{
    public class AddNhomHocPhanViewModel
    {
        [Required]
        public string MaCtdt { get; set; } = null!;

        [Required]
        [Display(Name = "Nhóm học phần")]
        public string MaNhomHp { get; set; } = null!;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số tín chỉ phải ≥ 0")]
        [Display(Name = "Tín chỉ bắt buộc")]
        public int SoTcBatBuoc { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số tín chỉ phải ≥ 0")]
        [Display(Name = "Tín chỉ tự chọn")]
        public int SoTcTuChon { get; set; }
        
      
        // Dùng để hiển thị dropdown
        public IEnumerable<SelectListItem> NhomHpList { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
