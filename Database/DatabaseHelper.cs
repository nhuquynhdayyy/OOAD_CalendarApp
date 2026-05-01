using Microsoft.Data.SqlClient;
using CalendarApp.Models;

namespace CalendarApp.Database
{
    public class DatabaseHelper
    {
        private const string ConnectionString =
            "Server=ADMIN;Database=CalendarApp_DB;Trusted_Connection=True;TrustServerCertificate=True;";

        public bool IsConnectionSuccessful() {
            using (SqlConnection connection = new SqlConnection(ConnectionString)) {
                try {
                    connection.Open();
                    return true; 
                }
                catch (Exception ex) {
                    Console.WriteLine("Lỗi kết nối: " + ex.Message);
                    return false;
                }
            }
        }
        public List<Appointment> GetAppointmentsByUserId(string userId)
        {
            var list = new List<Appointment>();
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            var cmd = new SqlCommand(
                "SELECT * FROM Appointments WHERE UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Appointment(
                    reader["Id"].ToString()!,
                    reader["Name"].ToString()!,
                    reader["Location"].ToString()!,
                    (DateTime)reader["StartTime"],
                    (DateTime)reader["EndTime"]
                ));
            }
            return list;
        }

        public void InsertAppointment(string userId, Appointment a)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
                INSERT INTO Appointments (Id, UserId, Name, Location, StartTime, EndTime)
                VALUES (@Id, @UserId, @Name, @Location, @StartTime, @EndTime)", conn);
            cmd.Parameters.AddWithValue("@Id", a.Id);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Name", a.Name);
            cmd.Parameters.AddWithValue("@Location", a.Location);
            cmd.Parameters.AddWithValue("@StartTime", a.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", a.EndTime);
            cmd.ExecuteNonQuery();
        }

        public void DeleteAppointment(string appointmentId)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            var deleteReminders = new SqlCommand(
                "DELETE FROM Reminders WHERE AppointmentId = @Id", conn);
            deleteReminders.Parameters.AddWithValue("@Id", appointmentId);
            deleteReminders.ExecuteNonQuery();

            var cmd = new SqlCommand(
                "DELETE FROM Appointments WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", appointmentId);
            cmd.ExecuteNonQuery();
        }
public void InsertReminder(Reminder r)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
                INSERT INTO Reminders (AppointmentId, AlertTime, Type)
                VALUES (@AppointmentId, @AlertTime, @Type)", conn);
            cmd.Parameters.AddWithValue("@AppointmentId", r.AppointmentId);
            cmd.Parameters.AddWithValue("@AlertTime", r.AlertTime);
            cmd.Parameters.AddWithValue("@Type", r.Type);
            cmd.ExecuteNonQuery();
        }

        public List<GroupMeeting> GetGroupMeetings()
        {
            var list = new List<GroupMeeting>();
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM GroupMeetings", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new GroupMeeting(
                    reader["Id"].ToString()!,
                    reader["Name"].ToString()!,
                    reader["Location"].ToString()!,
                    (DateTime)reader["StartTime"],
                    (DateTime)reader["EndTime"],
                    reader["MeetingCode"].ToString()!
                ));
            }
            return list;
        }

        public List<GroupMeeting> GetGroupMeetingsByUserId(string userId)
        {
            var list = new List<GroupMeeting>();
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
                SELECT gm.*
                FROM GroupMeetings gm
                INNER JOIN GroupMeetingParticipants gmp
                    ON gm.Id = gmp.MeetingId
                WHERE gmp.UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new GroupMeeting(
                    reader["Id"].ToString()!,
                    reader["Name"].ToString()!,
                    reader["Location"].ToString()!,
                    (DateTime)reader["StartTime"],
                    (DateTime)reader["EndTime"],
                    reader["MeetingCode"].ToString()!
                ));
            }
            return list;
        }

        public void AddParticipantToGroup(string meetingId, string userId)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
                IF NOT EXISTS (
                    SELECT 1 FROM GroupMeetingParticipants
                    WHERE MeetingId = @MeetingId AND UserId = @UserId
                )
                BEGIN
                    INSERT INTO GroupMeetingParticipants (MeetingId, UserId)
                    VALUES (@MeetingId, @UserId)
                END", conn);
cmd.Parameters.AddWithValue("@MeetingId", meetingId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.ExecuteNonQuery();
        }
    }
}
