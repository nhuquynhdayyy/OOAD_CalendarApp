using CalendarApp.Models;

namespace CalendarApp.Services
{
    public class ConflictHandler
    {
        public string ConflictType { get; set; }

        private List<Appointment> _appointments;
        private List<GroupMeeting> _groupMeetings;

        public ConflictHandler(List<Appointment> appointments, List<GroupMeeting> groupMeetings)
        {
            _appointments = appointments;
            _groupMeetings = groupMeetings;
            ConflictType = "";
        }

        public bool DetectConflict(Appointment a)
        {
            foreach (var existing in _appointments)
            {
                if (existing.Id != a.Id && existing.ConflictsWith(a))
                {
                    ConflictType = "overlap";
                    return true;
                }
            }
            return false;
        }
        public void PromptReplace()
        {
        }

        public GroupMeeting? DetectGroupMatch(Appointment a)
        {
            foreach (var gm in _groupMeetings)
            {
                if (gm.MatchesExisting(a))
                    return gm;
            }
            return null;
        }

        public void PromptJoinGroup()
        {
        }
    }
}
