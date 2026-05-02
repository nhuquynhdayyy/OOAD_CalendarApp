// using Microsoft.Data.Sqlite;
// using CalendarApp.Models;

// namespace CalendarApp.Database
// {
//     public class DatabaseHelper
//     {
//         private const string ConnectionString = "Data Source=CalendarApp.db";

//         public DatabaseHelper()
//         {
//             InitializeDatabase();
//         }

//         private void InitializeDatabase()
//         {
//             using var conn = new SqliteConnection(ConnectionString);
//             conn.Open();
//             var cmd = new SqliteCommand(@"
//                 CREATE TABLE IF NOT EXISTS Users (
//                     UserId TEXT PRIMARY KEY,
//                     Name TEXT NOT NULL,
//                     Email TEXT
//                 );
//                 CREATE TABLE IF NOT EXISTS Appointments (
//                     Id TEXT PRIMARY KEY,
//                     UserId TEXT NOT NULL REFERENCES Users(UserId),
//                     Name TEXT NOT NULL,
//                     Location TEXT,
//                     StartTime TEXT NOT NULL,
//                     EndTime TEXT NOT NULL
//                 );
//                 CREATE TABLE IF NOT EXISTS Reminders (
//                     Id INTEGER PRIMARY KEY AUTOINCREMENT,
//                     AppointmentId TEXT NOT NULL REFERENCES Appointments(Id),
//                     AlertTime TEXT NOT NULL,
//                     Type TEXT
//                 );
//                 CREATE TABLE IF NOT EXISTS GroupMeetings (
//                     Id TEXT PRIMARY KEY,
//                     Name TEXT NOT NULL,
//                     Location TEXT,
//                     StartTime TEXT NOT NULL,
//                     EndTime TEXT NOT NULL,
//                     MeetingCode TEXT
//                 );
//                 CREATE TABLE IF NOT EXISTS GroupMeetingParticipants (
//                     MeetingId TEXT NOT NULL REFERENCES GroupMeetings(Id),
//                     UserId TEXT NOT NULL REFERENCES Users(UserId),
//                     PRIMARY KEY (MeetingId, UserId)
//                 );
                
//                 -- Seed data if empty
//                 INSERT OR IGNORE INTO Users (UserId, Name, Email) VALUES ('u001', 'Nguyen Van A', 'a@email.com');
//                 INSERT OR IGNORE INTO GroupMeetings (Id, Name, Location, StartTime, EndTime, MeetingCode) 
//                 VALUES ('gm001', 'Team Standup', 'Room 101', '2026-05-06 09:00', '2026-05-06 09:30', 'STAND001');
//             ", conn);
//             cmd.ExecuteNonQuery();
//         }

//         public bool IsConnectionSuccessful() {
//             using (SqliteConnection connection = new SqliteConnection(ConnectionString)) {
//                 try {
//                     connection.Open();
//                     return true; 
//                 }
//                 catch (Exception ex) {
//                     Console.WriteLine("Lỗi kết nối SQLite: " + ex.Message);
//                     return false;
//                 }
//             }
//         }

//         public List<Appointment> GetAppointmentsByUserId(string userId)
//         {
//             var list = new List<Appointment>();
//             using var conn = new SqliteConnection(ConnectionString);
//             conn.Open();
//             var cmd = new SqliteCommand("SELECT * FROM Appointments WHERE UserId = @UserId", conn);
//             cmd.Parameters.AddWithValue("@UserId", userId);
//             using var reader = cmd.ExecuteReader();
//             while (reader.Read())
//             {
//                 list.Add(new Appointment(
//                     reader["Id"].ToString()!,
//                     reader["Name"].ToString()!,
//                     reader["Location"].ToString()!,
//                     DateTime.Parse(reader["StartTime"].ToString()!),
//                     DateTime.Parse(reader["EndTime"].ToString()!)
//                 ));
//             }
//             return list;
//         }

//         public void InsertAppointment(string userId, Appointment a)
//         {
//             using var conn = new SqliteConnection(ConnectionString);
//             conn.Open();
//             var cmd = new SqliteCommand(@"
//                 INSERT INTO Appointments (Id, UserId, Name, Location, StartTime, EndTime)
//                 VALUES (@Id, @UserId, @Name, @Location, @StartTime, @EndTime)", conn);
//             cmd.Parameters.AddWithValue("@Id", a.Id);
//             cmd.Parameters.AddWithValue("@UserId", userId);
//             cmd.Parameters.AddWithValue("@Name", a.Name);
//             cmd.Parameters.AddWithValue("@Location", a.Location);
//             cmd.Parameters.AddWithValue("@StartTime", a.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
//             cmd.Parameters.AddWithValue("@EndTime", a.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
//             cmd.ExecuteNonQuery();
//         }

//         public void DeleteAppointment(string appointmentId)
//         {
//             using var conn = new SqliteConnection(ConnectionString);
//             conn.Open();
//             var deleteReminders = new SqliteCommand("DELETE FROM Reminders WHERE AppointmentId = @Id", conn);
//             deleteReminders.Parameters.AddWithValue("@Id", appointmentId);
//             deleteReminders.ExecuteNonQuery();

//             var cmd = new SqliteCommand("DELETE FROM Appointments WHERE Id = @Id", conn);
//             cmd.Parameters.AddWithValue("@Id", appointmentId);
//             cmd.ExecuteNonQuery();
//         }

//         public void InsertReminder(Reminder r)
//         {
//             using var conn = new SqliteConnection(ConnectionString);
//             conn.Open();
//             var cmd = new SqliteCommand(@"
//                 INSERT INTO Reminders (AppointmentId, AlertTime, Type)
//                 VALUES (@AppointmentId, @AlertTime, @Type)", conn);
//             cmd.Parameters.AddWithValue("@AppointmentId", r.AppointmentId);
//             cmd.Parameters.AddWithValue("@AlertTime", r.AlertTime.ToString("yyyy-MM-dd HH:mm:ss"));
//             cmd.Parameters.AddWithValue("@Type", r.Type);
//             cmd.ExecuteNonQuery();
//         }

//         public List<GroupMeeting> GetGroupMeetings()
//         {
//             var list = new List<GroupMeeting>();
//             using var conn = new SqliteConnection(ConnectionString);
//             conn.Open();
//             var cmd = new SqliteCommand("SELECT * FROM GroupMeetings", conn);
//             using var reader = cmd.ExecuteReader();
//             while (reader.Read())
//             {
//                 list.Add(new GroupMeeting(
//                     reader["Id"].ToString()!,
//                     reader["Name"].ToString()!,
//                     reader["Location"].ToString()!,
//                     DateTime.Parse(reader["StartTime"].ToString()!),
//                     DateTime.Parse(reader["EndTime"].ToString()!),
//                     reader["MeetingCode"].ToString()!
//                 ));
//             }
//             return list;
//         }

//         public List<GroupMeeting> GetGroupMeetingsByUserId(string userId)
//         {
//             var list = new List<GroupMeeting>();
//             using var conn = new SqliteConnection(ConnectionString);
//             conn.Open();
//             var cmd = new SqliteCommand(@"
//                 SELECT gm.*
//                 FROM GroupMeetings gm
//                 INNER JOIN GroupMeetingParticipants gmp
//                     ON gm.Id = gmp.MeetingId
//                 WHERE gmp.UserId = @UserId", conn);
//             cmd.Parameters.AddWithValue("@UserId", userId);
//             using var reader = cmd.ExecuteReader();
//             while (reader.Read())
//             {
//                 list.Add(new GroupMeeting(
//                     reader["Id"].ToString()!,
//                     reader["Name"].ToString()!,
//                     reader["Location"].ToString()!,
//                     DateTime.Parse(reader["StartTime"].ToString()!),
//                     DateTime.Parse(reader["EndTime"].ToString()!),
//                     reader["MeetingCode"].ToString()!
//                 ));
//             }
//             return list;
//         }

//         public void AddParticipantToGroup(string meetingId, string userId)
//         {
//             using var conn = new SqliteConnection(ConnectionString);
//             conn.Open();
//             var cmd = new SqliteCommand(@"
//                 INSERT OR IGNORE INTO GroupMeetingParticipants (MeetingId, UserId)
//                 VALUES (@MeetingId, @UserId)", conn);
//             cmd.Parameters.AddWithValue("@MeetingId", meetingId);
//             cmd.Parameters.AddWithValue("@UserId", userId);
//             cmd.ExecuteNonQuery();
//         }

//         public void UpdateAppointment(Appointment a)
//         {
//             using var conn = new SqliteConnection(ConnectionString);
//             conn.Open();
//             var cmd = new SqliteCommand(@"
//                 UPDATE Appointments 
//                 SET Name = @Name, Location = @Location, StartTime = @StartTime, EndTime = @EndTime
//                 WHERE Id = @Id", conn);
//             cmd.Parameters.AddWithValue("@Id", a.Id);
//             cmd.Parameters.AddWithValue("@Name", a.Name);
//             cmd.Parameters.AddWithValue("@Location", a.Location);
//             cmd.Parameters.AddWithValue("@StartTime", a.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
//             cmd.Parameters.AddWithValue("@EndTime", a.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
//             cmd.ExecuteNonQuery();
//         }
//     }
// }
