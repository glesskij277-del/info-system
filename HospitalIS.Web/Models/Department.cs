using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Web.Models;

public class Department
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название отделения")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Название отделения должно содержать от 3 до 100 символов")]
    [Display(Name = "Название отделения")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите ФИО заведующего")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "ФИО заведующего должно содержать от 5 до 150 символов")]
    [Display(Name = "Заведующий")]
    public string HeadFullName { get; set; } = string.Empty;
}

