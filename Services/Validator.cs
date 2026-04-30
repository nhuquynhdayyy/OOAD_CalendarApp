using CalendarApp.Models;

namespace CalendarApp.Services
{
    public class Validator
    {
        public List<string> Rules { get; set; }

        public Validator()
        {
            Rules = new List<string>();
        }

        public bool CheckName(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        public bool CheckDuration(DateTime s, DateTime e)
        {
            return e > s;
        }

        public bool CheckTime(DateTime s, DateTime e)
        {
            return s < e;
        }

        public bool Validate(Appointment a)
        {
            return CheckName(a.Name)
                && CheckDuration(a.StartTime, a.EndTime)
                && CheckTime(a.StartTime, a.EndTime);
        }
    }
}
