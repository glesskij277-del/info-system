using HospitalIS.Web.Models;
using System.Text.RegularExpressions;

namespace HospitalIS.Web.Infrastructure;

public static class InputSanitizer
{
    public static void NormalizePatient(Patient patient)
    {
        patient.FullName = CollapseWhitespace(patient.FullName);
        patient.Address = CollapseWhitespace(patient.Address);
        patient.Phone = NormalizePhone(patient.Phone);
        patient.OmsPolicyNumber = DigitsOnly(patient.OmsPolicyNumber);
        patient.Snils = NormalizeSnils(patient.Snils);
    }

    public static void NormalizeDoctor(Doctor doctor)
    {
        doctor.FullName = CollapseWhitespace(doctor.FullName);
        doctor.Specialty = CollapseWhitespace(doctor.Specialty);
        doctor.OfficeNumber = CollapseWhitespace(doctor.OfficeNumber).ToUpperInvariant();
        doctor.Phone = NormalizePhone(doctor.Phone);
    }

    public static void NormalizeMedicalRecord(MedicalRecord record)
    {
        record.CardNumber = CollapseWhitespace(record.CardNumber).ToUpperInvariant();
        record.DiseaseHistory = CollapseWhitespace(record.DiseaseHistory);
    }

    public static void NormalizeDepartment(Department department)
    {
        department.Name = CollapseWhitespace(department.Name);
        department.HeadFullName = CollapseWhitespace(department.HeadFullName);
    }

    public static void NormalizeAppointment(Appointment appointment)
    {
        appointment.Diagnosis = CollapseWhitespace(appointment.Diagnosis);
        appointment.AppointmentDateTime = DateTime.SpecifyKind(appointment.AppointmentDateTime, DateTimeKind.Unspecified);
    }

    private static string NormalizePhone(string value)
    {
        var normalized = CollapseWhitespace(value).Replace("(", string.Empty).Replace(")", string.Empty);
        normalized = normalized.Replace("--", "-");
        return normalized;
    }

    private static string NormalizeSnils(string value)
    {
        var digits = DigitsOnly(value);

        if (digits.Length != 11)
        {
            return CollapseWhitespace(value);
        }

        return $"{digits[..3]}-{digits.Substring(3, 3)}-{digits.Substring(6, 3)} {digits[9..11]}";
    }

    private static string DigitsOnly(string value)
    {
        return Regex.Replace(value ?? string.Empty, "[^0-9]", string.Empty);
    }

    private static string CollapseWhitespace(string value)
    {
        var raw = value ?? string.Empty;
        return Regex.Replace(raw.Trim(), "\\s+", " ");
    }
}

