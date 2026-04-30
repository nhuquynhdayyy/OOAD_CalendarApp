using CalendarApp.Models;

namespace CalendarApp.Services
{
    public class Calendar
    {
        public string UserId { get; set; }
        public List<Appointment> Appointments { get; set; }
        public List<Reminder> Reminders { get; set; }
        private List<GroupMeeting> _groupMeetings;

        public Calendar(string userId)
        {
            UserId = userId;
            Appointments = new List<Appointment>();
            Reminders = new List<Reminder>();
            _groupMeetings = new List<GroupMeeting>();
        }

        public void AddAppointment(Appointment a)
        {
            Appointments.Add(a);
        }

        public void ReplaceAppointment(Appointment oldAppt, Appointment newAppt)
        {
            Appointments.Remove(oldAppt);
            Appointments.Add(newAppt);
        }

        public bool CheckConflict(Appointment a)
        {
            var handler = new ConflictHandler(Appointments, _groupMeetings);
            return handler.DetectConflict(a);
        }

        public Appointment? GetConflictingAppointment(Appointment a)
        {
            return Appointments.FirstOrDefault(existing =>
                existing.Id != a.Id && existing.ConflictsWith(a));
        }

        public GroupMeeting? FindGroupMatch(Appointment a)
        {
            var handler = new ConflictHandler(Appointments, _groupMeetings);
            return handler.DetectGroupMatch(a);
        }

        public GroupMeeting AddParticipantToGroup(GroupMeeting gm, User user)
        {
            gm.AddParticipant(user);
            return gm;
        }

        public void AddReminder(Reminder r)
        {
            Reminders.Add(r);
        }

        public void AddGroupMeeting(GroupMeeting gm)
        {
            _groupMeetings.Add(gm);
            Appointments.Add(gm);
        }

        public List<GroupMeeting> GetGroupMeetings()
        {
            return _groupMeetings;
        }
    }
}
