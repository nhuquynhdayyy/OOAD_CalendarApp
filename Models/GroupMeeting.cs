namespace CalendarApp.Models
{
    public class GroupMeeting : Appointment
    {
        public List<User> Participants { get; set; }
        public string MeetingCode { get; set; }

        public GroupMeeting(string id, string name, string location, DateTime startTime, DateTime endTime, string meetingCode)
            : base(id, name, location, startTime, endTime)
        {
            MeetingCode = meetingCode;
            Participants = new List<User>();
        }

        public void AddParticipant(User u)
        {
            if (!Participants.Any(p => p.UserId == u.UserId))
                Participants.Add(u);
        }

        public bool MatchesExisting(Appointment a)
        {
            return Name == a.Name
                && StartTime == a.StartTime
                && EndTime == a.EndTime;
        }

        public List<User> GetParticipants()
        {
            return Participants;
        }
    }
}
