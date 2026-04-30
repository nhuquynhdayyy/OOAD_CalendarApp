using CalendarApp.Services;
using CalendarApp.Models;

namespace CalendarApp.Models
{
    public class AddAppointmentWindow
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        private Validator _validator;

        public AddAppointmentWindow(string name, string location, DateTime startTime, DateTime endTime)
        {
            Name = name;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            _validator = new Validator();
        }

        public bool ValidateInput()
        {
            var appt = new Appointment(
                "temp", Name, Location, StartTime, EndTime);
            return _validator.Validate(appt);
        }

        public Appointment Submit()
        {
            return new Appointment(
                Guid.NewGuid().ToString(),
                Name,
                Location,
                StartTime,
                EndTime
            );
        }
    }
}
