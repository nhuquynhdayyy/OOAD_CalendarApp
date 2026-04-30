namespace CalendarApp.Models
{
    public class Reminder
    {
        public string AppointmentId { get; set; }
        public DateTime AlertTime { get; set; }
        public string Type { get; set; } 

        public Reminder(string appointmentId, DateTime alertTime, string type)
        {
            AppointmentId = appointmentId;
            AlertTime = alertTime;
            Type = type;
        }

        public void Trigger()
        {
            Console.WriteLine($"Reminder triggered for appointment {AppointmentId} at {AlertTime}");
        }

        public void Cancel()
        {
            Console.WriteLine($"Reminder cancelled for appointment {AppointmentId}");
        }
    }
}
