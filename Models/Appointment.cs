namespace CalendarApp.Models
{
    public class Appointment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public Appointment(string id, string name, string location, DateTime startTime, DateTime endTime)
        {
            Id = id;
            Name = name;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
        }

        public int GetDuration()
        {
            return (int)(EndTime - StartTime).TotalMinutes;
        }

        public bool ConflictsWith(Appointment other)
        {
            return this.StartTime < other.EndTime && this.EndTime > other.StartTime;
        }
    }
}
