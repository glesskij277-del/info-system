using System.Globalization;
using System.Text;
using HospitalIS.Web.Data;
using HospitalIS.Web.Models;
using HospitalIS.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Web.Controllers;

public class ReportsController(HospitalContext context) : Controller
{
    public async Task<IActionResult> Index(DateOnly? dateFrom, DateOnly? dateTo, int? doctorId, int? patientId)
    {
        if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
        {
            ModelState.AddModelError(string.Empty, "Дата 'с' не может быть больше даты 'по'.");
        }

        await FillFilterSelectLists(doctorId, patientId);

        var filteredAppointments = BuildAppointmentsQuery(dateFrom, dateTo, doctorId, patientId);

        var todayStart = DateTime.Today;
        var tomorrowStart = todayStart.AddDays(1);
        var weekEnd = todayStart.AddDays(7);
        var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        var model = new ReportsDashboardViewModel
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            DoctorId = doctorId,
            PatientId = patientId,
            FilteredAppointmentsCount = await filteredAppointments.CountAsync(),
            TodayAppointmentsCount = await context.Appointments
                .AsNoTracking()
                .CountAsync(a => a.AppointmentDateTime >= todayStart && a.AppointmentDateTime < tomorrowStart),
            UpcomingWeekAppointmentsCount = await context.Appointments
                .AsNoTracking()
                .CountAsync(a => a.AppointmentDateTime >= todayStart && a.AppointmentDateTime < weekEnd),
            CurrentMonthAppointmentsCount = await context.Appointments
                .AsNoTracking()
                .CountAsync(a => a.AppointmentDateTime >= monthStart && a.AppointmentDateTime < nextMonthStart),
            TopDoctors = await filteredAppointments
                .Join(
                    context.Doctors.AsNoTracking(),
                    appointment => appointment.DoctorId,
                    doctor => doctor.Id,
                    (appointment, doctor) => new { doctor.Id, doctor.FullName, doctor.Specialty })
                .GroupBy(x => new { x.Id, x.FullName, x.Specialty })
                .Select(group => new ReportDoctorWorkloadItem
                {
                    DoctorId = group.Key.Id,
                    DoctorFullName = group.Key.FullName,
                    Specialty = group.Key.Specialty,
                    AppointmentsCount = group.Count()
                })
                .OrderByDescending(item => item.AppointmentsCount)
                .ThenBy(item => item.DoctorFullName)
                .Take(10)
                .ToListAsync(),
            UpcomingAppointments = await filteredAppointments
                .Where(a => a.AppointmentDateTime >= DateTime.Now)
                .Join(
                    context.Patients.AsNoTracking(),
                    appointment => appointment.PatientId,
                    patient => patient.Id,
                    (appointment, patient) => new { appointment, patient.FullName })
                .Join(
                    context.Doctors.AsNoTracking(),
                    row => row.appointment.DoctorId,
                    doctor => doctor.Id,
                    (row, doctor) => new ReportAppointmentItem
                    {
                        AppointmentId = row.appointment.Id,
                        AppointmentDateTime = row.appointment.AppointmentDateTime,
                        PatientFullName = row.FullName,
                        DoctorFullName = doctor.FullName,
                        Diagnosis = row.appointment.Diagnosis
                    })
                .OrderBy(item => item.AppointmentDateTime)
                .Take(12)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> ExportAppointmentsCsv(DateOnly? dateFrom, DateOnly? dateTo, int? doctorId, int? patientId)
    {
        if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
        {
            TempData["Error"] = "Экспорт не выполнен: дата 'с' не может быть больше даты 'по'.";
            return RedirectToAction(nameof(Index), new { dateFrom, dateTo, doctorId, patientId });
        }

        var rows = await BuildAppointmentsQuery(dateFrom, dateTo, doctorId, patientId)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .OrderBy(a => a.AppointmentDateTime)
            .Select(a => new
            {
                a.Id,
                a.AppointmentDateTime,
                PatientName = a.Patient != null ? a.Patient.FullName : string.Empty,
                DoctorName = a.Doctor != null ? a.Doctor.FullName : string.Empty,
                a.Diagnosis
            })
            .ToListAsync();

        var lines = new StringBuilder();
        lines.AppendLine("ID;Дата и время;Пациент;Врач;Диагноз");

        foreach (var row in rows)
        {
            lines.Append(row.Id).Append(';')
                .Append(EscapeCsv(row.AppointmentDateTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture))).Append(';')
                .Append(EscapeCsv(row.PatientName)).Append(';')
                .Append(EscapeCsv(row.DoctorName)).Append(';')
                .Append(EscapeCsv(row.Diagnosis)).AppendLine();
        }

        return BuildCsvFile(lines.ToString(), "appointments-report");
    }

    public async Task<IActionResult> ExportPatientsCsv()
    {
        var rows = await context.Patients
            .AsNoTracking()
            .OrderBy(p => p.FullName)
            .Select(p => new
            {
                p.Id,
                p.FullName,
                p.DateOfBirth,
                p.Gender,
                p.Address,
                p.Phone,
                p.OmsPolicyNumber,
                p.Snils,
                AppointmentsCount = p.Appointments.Count,
                HasMedicalRecord = p.MedicalRecord != null
            })
            .ToListAsync();

        var lines = new StringBuilder();
        lines.AppendLine("ID;ФИО;Дата рождения;Пол;Адрес;Телефон;Полис ОМС;СНИЛС;Кол-во приемов;Медкарта");

        foreach (var row in rows)
        {
            lines.Append(row.Id).Append(';')
                .Append(EscapeCsv(row.FullName)).Append(';')
                .Append(EscapeCsv(row.DateOfBirth.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture))).Append(';')
                .Append(EscapeCsv(row.Gender.ToString())).Append(';')
                .Append(EscapeCsv(row.Address)).Append(';')
                .Append(EscapeCsv(row.Phone)).Append(';')
                .Append(EscapeCsv(row.OmsPolicyNumber)).Append(';')
                .Append(EscapeCsv(row.Snils)).Append(';')
                .Append(row.AppointmentsCount).Append(';')
                .Append(EscapeCsv(row.HasMedicalRecord ? "Да" : "Нет")).AppendLine();
        }

        return BuildCsvFile(lines.ToString(), "patients-report");
    }

    private IQueryable<Appointment> BuildAppointmentsQuery(DateOnly? dateFrom, DateOnly? dateTo, int? doctorId, int? patientId)
    {
        var query = context.Appointments
            .AsNoTracking()
            .AsQueryable();

        if (doctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == doctorId.Value);
        }

        if (patientId.HasValue)
        {
            query = query.Where(a => a.PatientId == patientId.Value);
        }

        if (dateFrom.HasValue)
        {
            var from = dateFrom.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(a => a.AppointmentDateTime >= from);
        }

        if (dateTo.HasValue)
        {
            var toExclusive = dateTo.Value.ToDateTime(TimeOnly.MinValue).AddDays(1);
            query = query.Where(a => a.AppointmentDateTime < toExclusive);
        }

        return query;
    }

    private async Task FillFilterSelectLists(int? doctorId = null, int? patientId = null)
    {
        var doctors = await context.Doctors
            .AsNoTracking()
            .OrderBy(d => d.FullName)
            .ToListAsync();

        var patients = await context.Patients
            .AsNoTracking()
            .OrderBy(p => p.FullName)
            .ToListAsync();

        ViewData["FilterDoctorId"] = new SelectList(doctors, "Id", "FullName", doctorId);
        ViewData["FilterPatientId"] = new SelectList(patients, "Id", "FullName", patientId);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var escaped = value.Replace("\"", "\"\"");
        var mustQuote = escaped.Contains(';') || escaped.Contains('"') || escaped.Contains('\n') || escaped.Contains('\r');

        return mustQuote ? $"\"{escaped}\"" : escaped;
    }

    private static FileContentResult BuildCsvFile(string csvContent, string filePrefix)
    {
        var utf8WithBom = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvContent)).ToArray();
        var fileName = $"{filePrefix}-{DateTime.Now:yyyyMMdd-HHmm}.csv";

        return new FileContentResult(utf8WithBom, "text/csv; charset=utf-8")
        {
            FileDownloadName = fileName
        };
    }
}

