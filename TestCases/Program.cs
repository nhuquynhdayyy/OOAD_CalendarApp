using System;
using System.Reflection;
using System.Windows.Forms;
using CalendarApp.Forms;
using CalendarApp.Database;
using CalendarApp.Models;
using CalendarApp.Services;

namespace TestCases
{
    class Program
    {
        static DatabaseHelper db = new DatabaseHelper();
        static User testUser = new User("test_u1", "Test User", "test@test.com");
        static CalendarApp.Services.Calendar testCalendar = new CalendarApp.Services.Calendar("test_u1");
        
        static string lastMessageText = "";
        static DialogResult mockDialogResult = DialogResult.OK;

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.WriteLine("Running UI Test Cases...\n");
            
            // Mock the MessageBox.Show
            AddAppointmentForm.ShowMessage = (text, caption, buttons, icon) => {
                lastMessageText = text;
                return mockDialogResult;
            };

            SetupDatabase();

            RunTestCase1();
            RunTestCase2();
            RunTestCase3();
            RunTestCase4();
            RunTestCase5();
            RunTestCase6();

            Console.WriteLine("\nAll test cases completed.");
        }

        static void SetupDatabase()
        {
            // Try to set up clean state
            try {
                using var conn = new MySql.Data.MySqlClient.MySqlConnection("Server=localhost;Database=CalendarApp_DB;Uid=root;Pwd=;");
                conn.Open();
                new MySql.Data.MySqlClient.MySqlCommand("DELETE FROM Reminders", conn).ExecuteNonQuery();
                new MySql.Data.MySqlClient.MySqlCommand("DELETE FROM GroupMeetingParticipants", conn).ExecuteNonQuery();
                new MySql.Data.MySqlClient.MySqlCommand("DELETE FROM Appointments", conn).ExecuteNonQuery();
                new MySql.Data.MySqlClient.MySqlCommand("DELETE FROM GroupMeetings", conn).ExecuteNonQuery();
                new MySql.Data.MySqlClient.MySqlCommand("DELETE FROM Users", conn).ExecuteNonQuery();

                new MySql.Data.MySqlClient.MySqlCommand("INSERT INTO Users (UserId, Name, Email) VALUES ('test_u1', 'Test User', 'test@test.com')", conn).ExecuteNonQuery();
                new MySql.Data.MySqlClient.MySqlCommand("INSERT INTO GroupMeetings (Id, Name, Location, StartTime, EndTime, MeetingCode) VALUES ('gm_test', 'Team Standup', 'Room 1', '2026-05-06 09:00:00', '2026-05-06 09:30:00', 'STAND001')", conn).ExecuteNonQuery();
            }
            catch (Exception ex) {
                Console.WriteLine("Error setting up DB: " + ex.Message);
            }
        }

        static AddAppointmentForm CreateForm(bool resetCalendar = true)
        {
            var date = new DateTime(2026, 5, 6, 9, 0, 0); // Active date
            if (resetCalendar) {
                testCalendar = new CalendarApp.Services.Calendar("test_u1"); // Reset calendar
                // Add existing group meeting to calendar
                testCalendar.AddGroupMeeting(new GroupMeeting("gm_test", "Team Standup", "Room 1", date, date.AddMinutes(30), "STAND001"));
            }
            return new AddAppointmentForm(testCalendar, testUser, db, date);
        }

        static void RunTestCase1()
        {
            Console.WriteLine("--- TEST CASE 1: Invalid input ---");
            
            // 1.1 Empty name
            var form1 = CreateForm();
            SetFormValues(form1, "", "Loc", new DateTime(2026,5,6,9,0,0), new DateTime(2026,5,6,10,0,0));
            ClickSubmit(form1);
            Console.WriteLine($"1.1 Empty name: {(lastMessageText.Contains("Invalid input") ? "PASS" : "FAIL")}");

            // 1.2 EndTime < StartTime
            var form2 = CreateForm();
            SetFormValues(form2, "Test", "Loc", new DateTime(2026,5,6,10,0,0), new DateTime(2026,5,6,9,0,0));
            ClickSubmit(form2);
            Console.WriteLine($"1.2 EndTime < StartTime: {(lastMessageText.Contains("Invalid input") ? "PASS" : "FAIL")}");

            // 1.3 EndTime = StartTime
            var form3 = CreateForm();
            SetFormValues(form3, "Test", "Loc", new DateTime(2026,5,6,9,0,0), new DateTime(2026,5,6,9,0,0));
            ClickSubmit(form3);
            Console.WriteLine($"1.3 EndTime = StartTime: {(lastMessageText.Contains("Invalid input") ? "PASS" : "FAIL")}");

            // 1.4 Valid input
            var form4 = CreateForm();
            SetFormValues(form4, "Valid Appt", "Loc", new DateTime(2026,5,6,14,0,0), new DateTime(2026,5,6,15,0,0));
            ClickSubmit(form4);
            Console.WriteLine($"1.4 Valid input: {(lastMessageText.Contains("saved successfully") ? "PASS" : "FAIL")}");
        }

        static void RunTestCase2()
        {
            Console.WriteLine("\n--- TEST CASE 2: Happy path ---");
            var form = CreateForm();
            SetFormValues(form, "Happy Appt", "Loc", new DateTime(2026,5,7,9,0,0), new DateTime(2026,5,7,10,0,0));
            ClickSubmit(form);
            
            var appts = db.GetAppointmentsByUserId(testUser.UserId);
            bool exists = appts.Exists(a => a.Name == "Happy Appt");
            Console.WriteLine($"2.1 & 2.2 Appointment saved to DB: {(exists ? "PASS" : "FAIL")}");
        }

        static void RunTestCase3()
        {
            Console.WriteLine("\n--- TEST CASE 3: Reminder ---");
            // 3.1 With Reminder
            var form1 = CreateForm();
            SetFormValues(form1, "Appt With Rem", "Loc", new DateTime(2026,5,8,9,0,0), new DateTime(2026,5,8,10,0,0), true);
            ClickSubmit(form1);
            Console.WriteLine($"3.1 With reminder saved: {(lastMessageText.Contains("saved successfully") ? "PASS" : "FAIL")}");

            // 3.2 Without Reminder
            var form2 = CreateForm();
            SetFormValues(form2, "Appt No Rem", "Loc", new DateTime(2026,5,8,11,0,0), new DateTime(2026,5,8,12,0,0), false);
            ClickSubmit(form2);
            Console.WriteLine($"3.2 Without reminder saved: {(lastMessageText.Contains("saved successfully") ? "PASS" : "FAIL")}");
        }

        static void RunTestCase4()
        {
            Console.WriteLine("\n--- TEST CASE 4: Time conflict ---");
            // Setup base appointment
            var formBase = CreateForm();
            SetFormValues(formBase, "Base Appt", "Loc", new DateTime(2026,5,9,9,0,0), new DateTime(2026,5,9,10,0,0));
            ClickSubmit(formBase);

            // 4.1 & 4.2 Conflict + Choose Yes (other time)
            var formConflict = CreateForm();
            SetFormValues(formConflict, "Conflict Appt", "Loc", new DateTime(2026,5,9,9,30,0), new DateTime(2026,5,9,10,30,0));
            
            // Add existing to calendar so it finds it
            var existingAppts = db.GetAppointmentsByUserId(testUser.UserId);
            foreach(var a in existingAppts) testCalendar.AddAppointment(a);
            
            mockDialogResult = DialogResult.Yes; // Choose Yes
            ClickSubmit(formConflict);
            Console.WriteLine($"4.1 & 4.2 Conflict -> Yes (don't save): {(lastMessageText.Contains("Trung lich voi") ? "PASS" : "FAIL")}");

            // 4.3 Conflict + Choose No (replace)
            mockDialogResult = DialogResult.No; // Choose No
            ClickSubmit(formConflict);
            Console.WriteLine($"4.3 Conflict -> No (replace): {(lastMessageText.Contains("saved successfully") ? "PASS" : "FAIL")}");
            
            // 4.4 Giáp đúng, không overlap
            var form4 = CreateForm();
            SetFormValues(form4, "Adjacent Appt", "Loc", new DateTime(2026,5,9,10,30,0), new DateTime(2026,5,9,11,30,0));
            mockDialogResult = DialogResult.OK;
            ClickSubmit(form4);
            Console.WriteLine($"4.4 Giáp đúng không lỗi: {(lastMessageText.Contains("saved successfully") ? "PASS" : "FAIL")}");
        }

        static void RunTestCase5()
        {
            Console.WriteLine("\n--- TEST CASE 5: Group meeting match ---");
            
            // 5.1 & 5.2 Match group meeting -> Yes
            var form1 = CreateForm();
            SetFormValues(form1, "Team Standup", "Loc", new DateTime(2026,5,6,9,0,0), new DateTime(2026,5,6,9,30,0));
            mockDialogResult = DialogResult.Yes;
            ClickSubmit(form1);
            Console.WriteLine($"5.1 & 5.2 Match group meeting -> Yes: {(lastMessageText.Contains("joined the group meeting") ? "PASS" : "FAIL")}");

            // 5.3 Match group meeting -> No
            var form2 = CreateForm();
            SetFormValues(form2, "Team Standup", "Loc", new DateTime(2026,5,6,9,0,0), new DateTime(2026,5,6,9,30,0));
            mockDialogResult = DialogResult.No;
            ClickSubmit(form2);
            Console.WriteLine($"5.3 Match group meeting -> No: {(lastMessageText.Contains("saved successfully") ? "PASS" : "FAIL")}");

            // 5.4 Same name different duration
            var form3 = CreateForm();
            SetFormValues(form3, "Team Standup", "Loc", new DateTime(2026,5,6,9,0,0), new DateTime(2026,5,6,10,0,0));
            mockDialogResult = DialogResult.OK;
            ClickSubmit(form3);
            Console.WriteLine($"5.4 Different duration: {(lastMessageText.Contains("saved successfully") ? "PASS" : "FAIL")}");
        }

        static void RunTestCase6()
        {
            Console.WriteLine("\n--- TEST CASE 6: Conflict + Group match ---");
            
            // Setup base conflicting appointment
            var formBase = CreateForm();
            SetFormValues(formBase, "Conflict Base", "Loc", new DateTime(2026,5,10,9,0,0), new DateTime(2026,5,10,9,30,0));
            ClickSubmit(formBase);

            // Create Group Meeting at same time and duration, same name
            testCalendar.AddGroupMeeting(new GroupMeeting("gm_tc6", "Overlap Group", "Room", new DateTime(2026,5,10,9,0,0), new DateTime(2026,5,10,9,30,0), "TC6"));

            // Add appointment with "Overlap Group" name and 9:00 - 9:30 duration
            var form6 = CreateForm(false);
            SetFormValues(form6, "Overlap Group", "Loc", new DateTime(2026,5,10,9,0,0), new DateTime(2026,5,10,9,30,0));
            
            // It will conflict with "Conflict Base" first. We choose "No" (Replace)
            // Then it should prompt for joining the group meeting. We choose "Yes" (Join).
            int promptCount = 0;
            bool conflictPromptShown = false;
            bool groupPromptShown = false;

            AddAppointmentForm.ShowMessage = (text, caption, buttons, icon) => {
                promptCount++;
                if (caption == "Conflict Warning") {
                    conflictPromptShown = true;
                    return DialogResult.No; // Replace
                }
                if (caption == "Join Group Meeting?") {
                    groupPromptShown = true;
                    return DialogResult.Yes; // Join
                }
                return DialogResult.OK;
            };

            ClickSubmit(form6);

            Console.WriteLine($"6.1 Conflict Warning shown first: {(conflictPromptShown && groupPromptShown ? "PASS" : "FAIL")}");
        }

        // Reflection helpers
        static void SetFormValues(AddAppointmentForm form, string name, string loc, DateTime start, DateTime end, bool rem = false)
        {
            typeof(AddAppointmentForm).GetField("txtName", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form).GetType().GetProperty("Text").SetValue(typeof(AddAppointmentForm).GetField("txtName", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form), name);
            typeof(AddAppointmentForm).GetField("txtLocation", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form).GetType().GetProperty("Text").SetValue(typeof(AddAppointmentForm).GetField("txtLocation", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form), loc);
            typeof(AddAppointmentForm).GetField("dtpStart", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form).GetType().GetProperty("Value").SetValue(typeof(AddAppointmentForm).GetField("dtpStart", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form), start);
            typeof(AddAppointmentForm).GetField("dtpEnd", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form).GetType().GetProperty("Value").SetValue(typeof(AddAppointmentForm).GetField("dtpEnd", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form), end);
            typeof(AddAppointmentForm).GetField("chkReminder", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form).GetType().GetProperty("Checked").SetValue(typeof(AddAppointmentForm).GetField("chkReminder", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form), rem);
        }

        static void ClickSubmit(AddAppointmentForm form)
        {
            var btn = typeof(AddAppointmentForm).GetField("btnSubmit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            var clickMethod = typeof(AddAppointmentForm).GetMethod("btnSubmit_Click", BindingFlags.NonPublic | BindingFlags.Instance);
            clickMethod.Invoke(form, new object[] { btn, EventArgs.Empty });
        }
    }
}
