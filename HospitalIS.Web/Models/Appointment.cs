using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Web.Models;

public class Appointment : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Дата и время приема")]
    [DataType(DataType.DateTime)]
    public DateTime AppointmentDateTime { get; set; }

    [Display(Name = "Пациент")]
    public int PatientId { get; set; }

    [Display(Name = "Врач")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Введите диагноз")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Диагноз должен содержать от 3 до 500 символов")]
    [Display(Name = "Диагноз")]
    public string Diagnosis { get; set; } = string.Empty;

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var minDate = new DateTime(2000, 1, 1);
        var maxDate = DateTime.Today.AddYears(5);

        if (AppointmentDateTime < minDate || AppointmentDateTime > maxDate)
        {
            yield return new ValidationResult(
                "Дата приема должна быть в диапазоне с 01.01.2000 по ближайшие 5 лет.",
                [nameof(AppointmentDateTime)]);
        }
    }
}

