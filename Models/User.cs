using CalendarApp.Services;

namespace CalendarApp.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public User(string userId, string name, string email)
        {
            UserId = userId;
            Name = name;
            Email = email;
        }

        public Calendar GetCalendar()
        {
            return new Calendar(this.UserId);
        }
    }
}
