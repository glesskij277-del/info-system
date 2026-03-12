using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Web.Models;

public class Patient : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите ФИО")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "ФИО должно содержать от 5 до 150 символов")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateOnly DateOfBirth { get; set; }

    [Display(Name = "Пол")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Введите адрес")]
    [StringLength(250)]
    [Display(Name = "Адрес")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите телефон")]
    [StringLength(20)]
    [RegularExpression(@"^\+?[0-9\-\s]{10,20}$", ErrorMessage = "Введите корректный телефон")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите полис ОМС")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Полис ОМС должен содержать 16 цифр")]
    [Display(Name = "Полис ОМС")]
    public string OmsPolicyNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите СНИЛС")]
    [RegularExpression(@"^\d{3}-\d{3}-\d{3} \d{2}$", ErrorMessage = "Формат СНИЛС: 000-000-000 00")]
    [Display(Name = "СНИЛС")]
    public string Snils { get; set; } = string.Empty;

    public ICollection<Appointment> Appointments { get; set; } = [];
    public MedicalRecord? MedicalRecord { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var minDate = today.AddYears(-130);

        if (DateOfBirth > today)
        {
            yield return new ValidationResult("Дата рождения не может быть в будущем.", [nameof(DateOfBirth)]);
        }

        if (DateOfBirth < minDate)
        {
            yield return new ValidationResult("Проверьте дату рождения: слишком ранняя дата.", [nameof(DateOfBirth)]);
        }
    }
}

