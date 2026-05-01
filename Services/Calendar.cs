using CalendarApp.Models;

namespace CalendarApp.Services
{
    public class Calendar
    {
        public string UserId { get; set; }
        public List<Appointment> Appointments { get; private set; }
        public List<Reminder> Reminders { get; private set; }
        public List<GroupMeeting> GroupMeetings { get; private set; }

        public Calendar(string userId)
        {
            UserId = userId;
            Appointments = new List<Appointment>();
            Reminders = new List<Reminder>();
            GroupMeetings = new List<GroupMeeting>();
        }

        public void AddAppointment(Appointment a)
        {
            if (!Appointments.Any(existing => existing.Id == a.Id))
            {
                Appointments.Add(a);
            }
        }

        public void RemoveAppointment(Appointment a)
        {
            Appointments.Remove(a);
        }

        public void ReplaceAppointment(Appointment oldAppt, Appointment newAppt)
        {
            Appointments.Remove(oldAppt);
            AddAppointment(newAppt);
        }

        public bool CheckConflict(Appointment a)
        {
            var handler = new ConflictHandler(Appointments, GroupMeetings);
            return handler.DetectConflict(a);
        }

        public Appointment? GetConflictingAppointment(Appointment a)
        {
            return Appointments.FirstOrDefault(existing =>
                existing.Id != a.Id && existing.ConflictsWith(a));
        }

        public GroupMeeting? FindGroupMatch(Appointment a)
        {
            var handler = new ConflictHandler(Appointments, GroupMeetings);
            return handler.DetectGroupMatch(a);
        }

        public GroupMeeting AddParticipantToGroup(GroupMeeting gm, User user)
        {
            gm.AddParticipant(user);
            AddAppointment(gm);
            return gm;
        }

        public void AddReminder(Reminder r)
        {
            Reminders.Add(r);
        }

        public void AddGroupMeeting(GroupMeeting gm)
        {
            if (!GroupMeetings.Any(existing => existing.Id == gm.Id))
            {
                GroupMeetings.Add(gm);
            }
        }

        public void AddJoinedGroupMeeting(GroupMeeting gm, User user)
        {
            var existing = GroupMeetings.FirstOrDefault(groupMeeting => groupMeeting.Id == gm.Id);
            if (existing == null)
            {
                existing = gm;
                GroupMeetings.Add(existing);
            }

            AddParticipantToGroup(existing, user);
        }

        public List<GroupMeeting> GetGroupMeetings()
        {
            return GroupMeetings;
        }
    }
}
