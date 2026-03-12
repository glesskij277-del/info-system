using HospitalIS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Web.Data;

public static class DbInitializer
{
    public static void Initialize(HospitalContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Departments.Any())
        {
            context.Departments.AddRange(
                new Department { Name = "Терапевтическое", HeadFullName = "Петров Сергей Иванович" },
                new Department { Name = "Хирургическое", HeadFullName = "Сидорова Анна Алексеевна" }
            );
        }

        if (!context.Doctors.Any())
        {
            context.Doctors.AddRange(
                new Doctor { FullName = "Иванов Алексей Петрович", Specialty = "Терапевт", OfficeNumber = "101", Phone = "+7-918-111-11-11" },
                new Doctor { FullName = "Кузнецова Мария Викторовна", Specialty = "Хирург", OfficeNumber = "205", Phone = "+7-918-222-22-22" }
            );
        }

        context.SaveChanges();

        if (!context.Patients.Any())
        {
            var patient = new Patient
            {
                FullName = "Смирнов Николай Андреевич",
                DateOfBirth = new DateOnly(1990, 5, 14),
                Gender = Gender.Male,
                Address = "ул. Центральная, д. 10",
                Phone = "+7-918-333-33-33",
                OmsPolicyNumber = "1234567890123456",
                Snils = "123-456-789 01"
            };

            context.Patients.Add(patient);
            context.SaveChanges();

            context.MedicalRecords.Add(new MedicalRecord
            {
                CardNumber = "MK-0001",
                PatientId = patient.Id,
                CreatedDate = DateOnly.FromDateTime(DateTime.Today),
                DiseaseHistory = "Хронические заболевания не выявлены."
            });

            var firstDoctorId = context.Doctors.OrderBy(d => d.Id).Select(d => d.Id).First();
            context.Appointments.Add(new Appointment
            {
                AppointmentDateTime = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Unspecified),
                PatientId = patient.Id,
                DoctorId = firstDoctorId,
                Diagnosis = "Профилактический осмотр"
            });
        }

        context.SaveChanges();
    }
}

