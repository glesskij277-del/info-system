namespace HospitalIS.Web.ViewModels;

public class HomeDashboardViewModel
{
    public int PatientsCount { get; set; }
    public int DoctorsCount { get; set; }
    public int AppointmentsCount { get; set; }
    public int MedicalRecordsCount { get; set; }
    public int DepartmentsCount { get; set; }

    public int TodayAppointmentsCount { get; set; }
    public int UpcomingWeekAppointmentsCount { get; set; }

    public IReadOnlyList<DashboardAppointmentItem> UpcomingAppointments { get; set; } = [];
    public IReadOnlyList<DashboardPatientItem> RecentPatients { get; set; } = [];
}

public class DashboardAppointmentItem
{
    public int Id { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
}

public class DashboardPatientItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string OmsPolicyNumber { get; set; } = string.Empty;
}

