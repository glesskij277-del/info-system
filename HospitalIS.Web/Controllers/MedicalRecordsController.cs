using HospitalIS.Web.Data;
using HospitalIS.Web.Infrastructure;
using HospitalIS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Web.Controllers;

public class MedicalRecordsController(HospitalContext context) : Controller
{
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var recordsQuery = context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Patient)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            recordsQuery = recordsQuery.Where(m =>
                m.CardNumber.ToLower().Contains(term) ||
                m.DiseaseHistory.ToLower().Contains(term) ||
                (m.Patient != null && m.Patient.FullName.ToLower().Contains(term)));
        }

        ViewData["SearchTerm"] = searchTerm;

        var records = await recordsQuery
            .OrderBy(m => m.CardNumber)
            .ToListAsync();

        return View(records);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var medicalRecord = await context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Patient)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medicalRecord == null)
        {
            return NotFound();
        }

        return View(medicalRecord);
    }

    public async Task<IActionResult> Create()
    {
        var hasPatients = await context.Patients.AnyAsync();
        if (!hasPatients)
        {
            TempData["Error"] = "Перед созданием медкарты нужно зарегистрировать пациента.";
            return RedirectToAction(nameof(Index));
        }

        await FillPatientsSelectList();
        return View(new MedicalRecord { CreatedDate = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CardNumber,PatientId,CreatedDate,DiseaseHistory")] MedicalRecord medicalRecord)
    {
        InputSanitizer.NormalizeMedicalRecord(medicalRecord);

        ModelState.Clear();
        TryValidateModel(medicalRecord);
        await ValidateMedicalRecordBusinessRules(medicalRecord);

        if (!ModelState.IsValid)
        {
            await FillPatientsSelectList(medicalRecord.PatientId);
            return View(medicalRecord);
        }

        try
        {
            context.Add(medicalRecord);
            await context.SaveChangesAsync();
            TempData["Success"] = "Медкарта успешно создана.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException exception) when (DbExceptionHelper.IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "Номер карты уже существует или у пациента уже есть медкарта.");
            await FillPatientsSelectList(medicalRecord.PatientId);
            return View(medicalRecord);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var medicalRecord = await context.MedicalRecords.FindAsync(id);
        if (medicalRecord == null)
        {
            return NotFound();
        }

        await FillPatientsSelectList(medicalRecord.PatientId);
        return View(medicalRecord);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CardNumber,PatientId,CreatedDate,DiseaseHistory")] MedicalRecord medicalRecord)
    {
        if (id != medicalRecord.Id)
        {
            return NotFound();
        }

        InputSanitizer.NormalizeMedicalRecord(medicalRecord);

        ModelState.Clear();
        TryValidateModel(medicalRecord);
        await ValidateMedicalRecordBusinessRules(medicalRecord);

        if (!ModelState.IsValid)
        {
            await FillPatientsSelectList(medicalRecord.PatientId);
            return View(medicalRecord);
        }

        try
        {
            context.Update(medicalRecord);
            await context.SaveChangesAsync();
            TempData["Success"] = "Медкарта обновлена.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MedicalRecordExists(medicalRecord.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException exception) when (DbExceptionHelper.IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "Номер карты уже существует или у пациента уже есть другая медкарта.");
            await FillPatientsSelectList(medicalRecord.PatientId);
            return View(medicalRecord);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var medicalRecord = await context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Patient)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medicalRecord == null)
        {
            return NotFound();
        }

        return View(medicalRecord);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var medicalRecord = await context.MedicalRecords.FindAsync(id);
        if (medicalRecord != null)
        {
            context.MedicalRecords.Remove(medicalRecord);
            await context.SaveChangesAsync();
            TempData["Success"] = "Медкарта удалена.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateMedicalRecordBusinessRules(MedicalRecord medicalRecord)
    {
        if (!await context.Patients.AnyAsync(p => p.Id == medicalRecord.PatientId))
        {
            ModelState.AddModelError(nameof(medicalRecord.PatientId), "Выберите существующего пациента.");
        }
    }

    private async Task FillPatientsSelectList(int? patientId = null)
    {
        var patients = await context.Patients
            .AsNoTracking()
            .OrderBy(p => p.FullName)
            .ToListAsync();

        ViewData["PatientId"] = new SelectList(patients, "Id", "FullName", patientId);
    }

    private bool MedicalRecordExists(int id)
    {
        return context.MedicalRecords.Any(e => e.Id == id);
    }
}

