using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Web.Models;

public class MedicalRecord : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите номер карты")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Номер карты должен содержать от 3 до 30 символов")]
    [Display(Name = "Номер карты")]
    public string CardNumber { get; set; } = string.Empty;

    [Display(Name = "Пациент")]
    public int PatientId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата заведения")]
    public DateOnly CreatedDate { get; set; }

    [Required(ErrorMessage = "Введите историю болезней")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "История болезней должна содержать от 10 до 2000 символов")]
    [Display(Name = "История болезней")]
    public string DiseaseHistory { get; set; } = string.Empty;

    public Patient? Patient { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var minDate = new DateOnly(2000, 1, 1);

        if (CreatedDate > today)
        {
            yield return new ValidationResult("Дата заведения не может быть в будущем.", [nameof(CreatedDate)]);
        }

        if (CreatedDate < minDate)
        {
            yield return new ValidationResult("Проверьте дату заведения медкарты.", [nameof(CreatedDate)]);
        }
    }
}

