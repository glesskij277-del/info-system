using HospitalIS.Web.Data;
using HospitalIS.Web.Infrastructure;
using HospitalIS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Web.Controllers;

public class DepartmentsController(HospitalContext context) : Controller
{
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var query = context.Departments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(d =>
                d.Name.ToLower().Contains(term) ||
                d.HeadFullName.ToLower().Contains(term));
        }

        ViewData["SearchTerm"] = searchTerm;

        var departments = await query
            .OrderBy(d => d.Name)
            .ToListAsync();

        return View(departments);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,HeadFullName")] Department department)
    {
        InputSanitizer.NormalizeDepartment(department);

        ModelState.Clear();
        TryValidateModel(department);

        if (!ModelState.IsValid)
        {
            return View(department);
        }

        try
        {
            context.Add(department);
            await context.SaveChangesAsync();
            TempData["Success"] = "Отделение успешно добавлено.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException exception) when (DbExceptionHelper.IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "Отделение с таким названием уже существует.");
            return View(department);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,HeadFullName")] Department department)
    {
        if (id != department.Id)
        {
            return NotFound();
        }

        InputSanitizer.NormalizeDepartment(department);

        ModelState.Clear();
        TryValidateModel(department);

        if (!ModelState.IsValid)
        {
            return View(department);
        }

        try
        {
            context.Update(department);
            await context.SaveChangesAsync();
            TempData["Success"] = "Отделение обновлено.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DepartmentExists(department.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException exception) when (DbExceptionHelper.IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "Отделение с таким названием уже существует.");
            return View(department);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await context.Departments.FindAsync(id);
        if (department != null)
        {
            context.Departments.Remove(department);
            await context.SaveChangesAsync();
            TempData["Success"] = "Отделение удалено.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool DepartmentExists(int id)
    {
        return context.Departments.Any(e => e.Id == id);
    }
}

