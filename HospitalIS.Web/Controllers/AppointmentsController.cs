using HospitalIS.Web.Data;
using HospitalIS.Web.Infrastructure;
using HospitalIS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Web.Controllers;

public class AppointmentsController(HospitalContext context) : Controller
{
    public async Task<IActionResult> Index(
        string? searchTerm,
        int? doctorId,
        int? patientId,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string? period = "all")
    {
        var query = context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(a =>
                a.Diagnosis.ToLower().Contains(term) ||
                (a.Patient != null && a.Patient.FullName.ToLower().Contains(term)) ||
                (a.Doctor != null && a.Doctor.FullName.ToLower().Contains(term)));
        }

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

        var now = DateTime.Now;
        var todayStart = DateTime.Today;
        var tomorrowStart = todayStart.AddDays(1);

        query = period switch
        {
            "today" => query.Where(a => a.AppointmentDateTime >= todayStart && a.AppointmentDateTime < tomorrowStart),
            "upcoming" => query.Where(a => a.AppointmentDateTime >= now),
            "past" => query.Where(a => a.AppointmentDateTime < now),
            _ => query
        };

        ViewData["SearchTerm"] = searchTerm;
        ViewData["SelectedDoctorId"] = doctorId;
        ViewData["SelectedPatientId"] = patientId;
        ViewData["DateFrom"] = dateFrom?.ToString("yyyy-MM-dd");
        ViewData["DateTo"] = dateTo?.ToString("yyyy-MM-dd");
        ViewData["Period"] = period;

        await FillFilterSelectLists(doctorId, patientId);

        var appointments = await query
            .OrderByDescending(a => a.AppointmentDateTime)
            .ToListAsync();

        return View(appointments);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var appointment = await context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (appointment == null)
        {
            return NotFound();
        }

        return View(appointment);
    }

    public async Task<IActionResult> Create()
    {
        var hasPatients = await context.Patients.AnyAsync();
        var hasDoctors = await context.Doctors.AnyAsync();

        if (!hasPatients || !hasDoctors)
        {
            TempData["Error"] = "Перед созданием приема нужно добавить хотя бы одного пациента и одного врача.";
            return RedirectToAction(nameof(Index));
        }

        await FillSelectLists();
        return View(new Appointment { AppointmentDateTime = DateTime.SpecifyKind(DateTime.Now.AddHours(1), DateTimeKind.Unspecified) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AppointmentDateTime,PatientId,DoctorId,Diagnosis")] Appointment appointment)
    {
        InputSanitizer.NormalizeAppointment(appointment);

        ModelState.Clear();
        TryValidateModel(appointment);
        await ValidateAppointmentBusinessRules(appointment);

        if (!ModelState.IsValid)
        {
            await FillSelectLists(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        try
        {
            context.Add(appointment);
            await context.SaveChangesAsync();
            TempData["Success"] = "Прием успешно добавлен.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException exception) when (DbExceptionHelper.IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "Конфликт расписания: врач или пациент уже записан на это время.");
            await FillSelectLists(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var appointment = await context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }

        await FillSelectLists(appointment.PatientId, appointment.DoctorId);
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,AppointmentDateTime,PatientId,DoctorId,Diagnosis")] Appointment appointment)
    {
        if (id != appointment.Id)
        {
            return NotFound();
        }

        InputSanitizer.NormalizeAppointment(appointment);

        ModelState.Clear();
        TryValidateModel(appointment);
        await ValidateAppointmentBusinessRules(appointment);

        if (!ModelState.IsValid)
        {
            await FillSelectLists(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        try
        {
            context.Update(appointment);
            await context.SaveChangesAsync();
            TempData["Success"] = "Прием обновлен.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AppointmentExists(appointment.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException exception) when (DbExceptionHelper.IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "Конфликт расписания: врач или пациент уже записан на это время.");
            await FillSelectLists(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var appointment = await context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (appointment == null)
        {
            return NotFound();
        }

        return View(appointment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appointment = await context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            context.Appointments.Remove(appointment);
            await context.SaveChangesAsync();
            TempData["Success"] = "Прием удален.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateAppointmentBusinessRules(Appointment appointment)
    {
        if (!await context.Patients.AnyAsync(p => p.Id == appointment.PatientId))
        {
            ModelState.AddModelError(nameof(appointment.PatientId), "Выберите существующего пациента.");
        }

        if (!await context.Doctors.AnyAsync(d => d.Id == appointment.DoctorId))
        {
            ModelState.AddModelError(nameof(appointment.DoctorId), "Выберите существующего врача.");
        }

        var doctorConflict = await context.Appointments.AnyAsync(a =>
            a.Id != appointment.Id &&
            a.DoctorId == appointment.DoctorId &&
            a.AppointmentDateTime == appointment.AppointmentDateTime);

        if (doctorConflict)
        {
            ModelState.AddModelError(nameof(appointment.AppointmentDateTime), "У врача уже есть прием на это время.");
        }

        var patientConflict = await context.Appointments.AnyAsync(a =>
            a.Id != appointment.Id &&
            a.PatientId == appointment.PatientId &&
            a.AppointmentDateTime == appointment.AppointmentDateTime);

        if (patientConflict)
        {
            ModelState.AddModelError(nameof(appointment.AppointmentDateTime), "Пациент уже записан на это время.");
        }
    }

    private async Task FillSelectLists(int? patientId = null, int? doctorId = null)
    {
        var patients = await context.Patients
            .AsNoTracking()
            .OrderBy(p => p.FullName)
            .ToListAsync();

        var doctors = await context.Doctors
            .AsNoTracking()
            .OrderBy(d => d.FullName)
            .ToListAsync();

        ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", patientId);
        ViewData["DoctorId"] = new SelectList(doctors, "Id", "FullName", doctorId);
    }

    private async Task FillFilterSelectLists(int? doctorId = null, int? patientId = null)
    {
        var patients = await context.Patients
            .AsNoTracking()
            .OrderBy(p => p.FullName)
            .ToListAsync();

        var doctors = await context.Doctors
            .AsNoTracking()
            .OrderBy(d => d.FullName)
            .ToListAsync();

        ViewData["FilterPatientId"] = new SelectList(patients, "Id", "FullName", patientId);
        ViewData["FilterDoctorId"] = new SelectList(doctors, "Id", "FullName", doctorId);
    }

    private bool AppointmentExists(int id)
    {
        return context.Appointments.Any(e => e.Id == id);
    }
}

