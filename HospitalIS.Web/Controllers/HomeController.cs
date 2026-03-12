using HospitalIS.Web.Data;
using HospitalIS.Web.Models;
using HospitalIS.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HospitalIS.Web.Controllers;

public class HomeController(HospitalContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var todayStart = DateTime.Today;
        var tomorrowStart = todayStart.AddDays(1);
        var weekEnd = todayStart.AddDays(7);

        var model = new HomeDashboardViewModel
        {
            PatientsCount = await context.Patients.AsNoTracking().CountAsync(),
            DoctorsCount = await context.Doctors.AsNoTracking().CountAsync(),
            AppointmentsCount = await context.Appointments.AsNoTracking().CountAsync(),
            MedicalRecordsCount = await context.MedicalRecords.AsNoTracking().CountAsync(),
            DepartmentsCount = await context.Departments.AsNoTracking().CountAsync(),
            TodayAppointmentsCount = await context.Appointments.AsNoTracking()
                .CountAsync(a => a.AppointmentDateTime >= todayStart && a.AppointmentDateTime < tomorrowStart),
            UpcomingWeekAppointmentsCount = await context.Appointments.AsNoTracking()
                .CountAsync(a => a.AppointmentDateTime >= todayStart && a.AppointmentDateTime < weekEnd),
            UpcomingAppointments = await context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDateTime >= DateTime.Now)
                .OrderBy(a => a.AppointmentDateTime)
                .Take(8)
                .Select(a => new DashboardAppointmentItem
                {
                    Id = a.Id,
                    AppointmentDateTime = a.AppointmentDateTime,
                    PatientName = a.Patient != null ? a.Patient.FullName : "-",
                    DoctorName = a.Doctor != null ? a.Doctor.FullName : "-",
                    Diagnosis = a.Diagnosis
                })
                .ToListAsync(),
            RecentPatients = await context.Patients
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Take(8)
                .Select(p => new DashboardPatientItem
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    Phone = p.Phone,
                    OmsPolicyNumber = p.OmsPolicyNumber
                })
                .ToListAsync()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

