using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Web.Models;

public class Doctor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите ФИО")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "ФИО должно содержать от 5 до 150 символов")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите специальность")]
    [StringLength(100)]
    [Display(Name = "Специальность")]
    public string Specialty { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите кабинет")]
    [StringLength(20)]
    [RegularExpression(@"^[A-Za-zА-Яа-я0-9\-\/]{1,20}$", ErrorMessage = "Укажите корректный номер кабинета")]
    [Display(Name = "Кабинет")]
    public string OfficeNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите телефон")]
    [StringLength(20)]
    [RegularExpression(@"^\+?[0-9\-\s]{10,20}$", ErrorMessage = "Введите корректный телефон")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    public ICollection<Appointment> Appointments { get; set; } = [];
}

