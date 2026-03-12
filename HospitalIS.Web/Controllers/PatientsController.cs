using HospitalIS.Web.Data;
using HospitalIS.Web.Infrastructure;
using HospitalIS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Web.Controllers;

public class PatientsController(HospitalContext context) : Controller
{
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var query = context.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.FullName.ToLower().Contains(term) ||
                p.Snils.Contains(term) ||
                p.OmsPolicyNumber.Contains(term) ||
                p.Phone.Contains(term));
        }

        ViewData["SearchTerm"] = searchTerm;

        var patients = await query
            .OrderBy(p => p.FullName)
            .ToListAsync();

        return View(patients);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await context.Patients
            .AsNoTracking()
            .Include(p => p.MedicalRecord)
            .Include(p => p.Appointments.OrderByDescending(a => a.AppointmentDateTime))
            .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (patient == null)
        {
            return NotFound();
        }

        return View(patient);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,FullName,DateOfBirth,Gender,Address,Phone,OmsPolicyNumber,Snils")] Patient patient)
    {
        InputSanitizer.NormalizePatient(patient);

        ModelState.Clear();
        TryValidateModel(patient);

        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        try
        {
            context.Add(patient);
            await context.SaveChangesAsync();
            TempData["Success"] = "Пациент успешно зарегистрирован.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException exception) when (DbExceptionHelper.IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "Пациент с таким полисом ОМС или СНИЛС уже существует.");
            return View(patient);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await context.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound();
        }

        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,DateOfBirth,Gender,Address,Phone,OmsPolicyNumber,Snils")] Patient patient)
    {
        if (id != patient.Id)
        {
            return NotFound();
        }

        InputSanitizer.NormalizePatient(patient);

        ModelState.Clear();
        TryValidateModel(patient);

        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        try
        {
            context.Update(patient);
            await context.SaveChangesAsync();
            TempData["Success"] = "Данные пациента обновлены.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PatientExists(patient.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException exception) when (DbExceptionHelper.IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "Не удалось обновить запись: ОМС или СНИЛС уже используются.");
            return View(patient);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (patient == null)
        {
            return NotFound();
        }

        ViewData["AppointmentsCount"] = await context.Appointments.CountAsync(a => a.PatientId == id);
        ViewData["HasMedicalRecord"] = await context.MedicalRecords.AnyAsync(m => m.PatientId == id);

        return View(patient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var patient = await context.Patients.FindAsync(id);

        if (patient == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var hasAppointments = await context.Appointments.AnyAsync(a => a.PatientId == id);
        if (hasAppointments)
        {
            TempData["Error"] = "Нельзя удалить пациента, пока у него есть связанные приемы.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        context.Patients.Remove(patient);
        await context.SaveChangesAsync();

        TempData["Success"] = "Пациент удален.";
        return RedirectToAction(nameof(Index));
    }

    private bool PatientExists(int id)
    {
        return context.Patients.Any(e => e.Id == id);
    }
}

