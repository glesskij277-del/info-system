namespace HospitalIS.Web.ViewModels;

public class ReportsDashboardViewModel
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int? DoctorId { get; set; }
    public int? PatientId { get; set; }

    public int FilteredAppointmentsCount { get; set; }
    public int TodayAppointmentsCount { get; set; }
    public int UpcomingWeekAppointmentsCount { get; set; }
    public int CurrentMonthAppointmentsCount { get; set; }

    public IReadOnlyList<ReportDoctorWorkloadItem> TopDoctors { get; set; } = [];
    public IReadOnlyList<ReportAppointmentItem> UpcomingAppointments { get; set; } = [];
}

public class ReportDoctorWorkloadItem
{
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int AppointmentsCount { get; set; }
}

public class ReportAppointmentItem
{
    public int AppointmentId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string DoctorFullName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
}

