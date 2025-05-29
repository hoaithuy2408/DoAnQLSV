using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace QLSV.ViewModel;

public class BulkCreateViewModel
{
    [Required]
    public string MaCtdt { get; set; }

    public List<MonHocSelection> MonHocSelections { get; set; } = new List<MonHocSelection>();

    public List<SelectListItem> LoaiMhList { get; set; }
    public List<SelectListItem> HkList { get; set; }
    public List<SelectListItem> NhList { get; set; }
    public List<SelectListItem> NhomHpList { get; set; }

}

public class MonHocSelection
{
    public string MaMh { get; set; }
    public string TenMh { get; set; }
    public bool IsSelected { get; set; }

    [RequiredIfSelected(nameof(IsSelected), ErrorMessage = "Vui lòng chọn loại môn học")]
    public bool? LoaiMh { get; set; }

    [RequiredIfSelected(nameof(IsSelected), ErrorMessage = "Vui lòng chọn học kỳ")]
    public string MaHk { get; set; }

    [RequiredIfSelected(nameof(IsSelected), ErrorMessage = "Vui lòng chọn năm học")]
    public string MaNh { get; set; }

    [RequiredIfSelected(nameof(IsSelected), ErrorMessage = "Vui lòng chọn năm học")]
    public string MaNhomHp { get; set; }
}
public class RequiredIfSelectedAttribute : ValidationAttribute
{
    private readonly string _propertyName;

    public RequiredIfSelectedAttribute(string propertyName)
    {
        _propertyName = propertyName;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var type = instance.GetType();
        var property = type.GetProperty(_propertyName);
        var propertyValue = (bool?)property.GetValue(instance);

        if (propertyValue == true && (value == null || value is string str && string.IsNullOrEmpty(str)))
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
