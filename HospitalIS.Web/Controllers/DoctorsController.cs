using HospitalIS.Web.Data;
using HospitalIS.Web.Infrastructure;
using HospitalIS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Web.Controllers;

public class DoctorsController(HospitalContext context) : Controller
{
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var query = context.Doctors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(d =>
                d.FullName.ToLower().Contains(term) ||
                d.Specialty.ToLower().Contains(term) ||
                d.Phone.Contains(term) ||
                d.OfficeNumber.ToLower().Contains(term));
        }

        ViewData["SearchTerm"] = searchTerm;

        var doctors = await query
            .OrderBy(d => d.FullName)
            .ToListAsync();

        return View(doctors);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await context.Doctors
            .AsNoTracking()
            .Include(d => d.Appointments.OrderByDescending(a => a.AppointmentDateTime))
            .ThenInclude(a => a.Patient)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,FullName,Specialty,OfficeNumber,Phone")] Doctor doctor)
    {
        InputSanitizer.NormalizeDoctor(doctor);

        ModelState.Clear();
        TryValidateModel(doctor);

        if (!ModelState.IsValid)
        {
            return View(doctor);
        }

        context.Add(doctor);
        await context.SaveChangesAsync();

        TempData["Success"] = "Врач успешно добавлен.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await context.Doctors.FindAsync(id);
        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Specialty,OfficeNumber,Phone")] Doctor doctor)
    {
        if (id != doctor.Id)
        {
            return NotFound();
        }

        InputSanitizer.NormalizeDoctor(doctor);

        ModelState.Clear();
        TryValidateModel(doctor);

        if (!ModelState.IsValid)
        {
            return View(doctor);
        }

        try
        {
            context.Update(doctor);
            await context.SaveChangesAsync();
            TempData["Success"] = "Данные врача обновлены.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DoctorExists(doctor.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (doctor == null)
        {
            return NotFound();
        }

        ViewData["AppointmentsCount"] = await context.Appointments.CountAsync(a => a.DoctorId == id);

        return View(doctor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var doctor = await context.Doctors.FindAsync(id);

        if (doctor == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var hasAppointments = await context.Appointments.AnyAsync(a => a.DoctorId == id);
        if (hasAppointments)
        {
            TempData["Error"] = "Нельзя удалить врача, пока у него есть связанные приемы.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        context.Doctors.Remove(doctor);
        await context.SaveChangesAsync();

        TempData["Success"] = "Врач удален.";
        return RedirectToAction(nameof(Index));
    }

    private bool DoctorExists(int id)
    {
        return context.Doctors.Any(e => e.Id == id);
    }
}

